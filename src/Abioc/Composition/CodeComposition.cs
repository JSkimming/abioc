// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Generates the code from a <see cref="CompositionContext"/>.
    /// </summary>
    public static class CodeComposition
    {
        private static readonly string NewLine = Environment.NewLine;

        private static readonly string DoubleNewLine = NewLine + NewLine;

        /// <summary>
        /// Generates the code from the composition <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="CompositionContext"/>.</param>
        /// <returns>The generated code from the composition <paramref name="context"/>.</returns>
        public static string GenerateCode(this CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IReadOnlyList<IComposition> compositions =
                context.Compositions.Values.OrderBy(r => r.Type.ToCompileName()).ToList();

            var code = new CodeCompositions(context.ConstructionContext);

            // First try with simple method names.
            foreach (IComposition composition in compositions)
            {
                Type type = composition.Type;
                string composeMethodName = composition.GetComposeMethodName(context, simpleName: true);
                bool requiresConstructionContext = composition.RequiresConstructionContext(context);

                code.ComposeMethods.Add((composeMethodName, type, requiresConstructionContext));
                code.Methods.AddRange(composition.GetMethods(context, simpleName: true));
                code.Fields.AddRange(composition.GetFields(context));
                code.FieldInitializations.AddRange(composition.GetFieldInitializations(context));
            }

            // Check if there are any name conflicts.
            if (code.ComposeMethods.Select(c => c.name).Distinct().Count() != code.ComposeMethods.Count)
            {
                code.ComposeMethods.Clear();
                code.Methods.Clear();

                // Now try with complex names, this should prevent conflicts.
                foreach (IComposition composition in compositions)
                {
                    Type type = composition.Type;
                    string composeMethodName = composition.GetComposeMethodName(context, simpleName: false);
                    bool requiresConstructionContext = composition.RequiresConstructionContext(context);

                    code.ComposeMethods.Add((composeMethodName, type, requiresConstructionContext));
                    code.Methods.AddRange(composition.GetMethods(context, simpleName: false));
                }
            }

            string generatedCode = GenerateCode(context, code);
            return generatedCode;
        }

        private static string GenerateCode(CompositionContext context, CodeCompositions code)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            var builder = new StringBuilder(10240);
            builder.AppendFormat(
                "namespace Abioc.Generated{0}{{{0}    public static class Construction{0}    {{",
                NewLine);

            string fieldsAndMethods = GenerateFieldsAndMethods(code);
            fieldsAndMethods = CodeGen.Indent(NewLine + fieldsAndMethods, 2);
            builder.Append(fieldsAndMethods);
            builder.Append(NewLine);

            string composeMapMethod = GenerateComposeMapMethod(code);
            composeMapMethod = CodeGen.Indent(NewLine + composeMapMethod, 2);
            builder.Append(composeMapMethod);

            builder.AppendFormat("{0}    }}{0}}}{0}", NewLine);

            var generatedCode = builder.ToString();
            return generatedCode;
        }

        private static string GenerateFieldsAndMethods(CodeCompositions code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string fields = string.Join(NewLine, code.Fields);
            string methods = string.Join(DoubleNewLine, code.Methods);
            string fieldsAndMethods = fields;
            if (fieldsAndMethods.Length > 0)
                fieldsAndMethods += DoubleNewLine;
            fieldsAndMethods = fieldsAndMethods + methods;

            return fieldsAndMethods;
        }

        private static string GenerateComposeMapMethod(CodeCompositions code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string composeMapType = code.HasConstructionContext
                ? $"System.Collections.Generic.Dictionary<System.Type, System.Func<{code.ConstructionContext}, object>>"
                : "System.Collections.Generic.Dictionary<System.Type, System.Func<object>>";

            var builder = new StringBuilder();
            builder.AppendFormat(
                "private static {0} GetCreateMap(){1}{{{1}    return new {0}{1}    {{",
                composeMapType,
                NewLine);

            string initializers =
                string.Join(
                    NewLine,
                    code.ComposeMethods.Select(c => GenerateComposeMapInitializer(code.HasConstructionContext, c)));
            initializers = CodeGen.Indent(NewLine + initializers, 2);
            builder.Append(initializers);

            builder.AppendFormat("{0}    }};{0}}}", NewLine);
            return builder.ToString();
        }

        private static string GenerateComposeMapInitializer(bool hasContext, (string name, Type type, bool requiresContext) data)
        {
            string key = $"typeof({data.type.ToCompileName()})";
            string value =
                hasContext ^ data.requiresContext
                    ? $"c => {data.name}()"
                    : data.name;

            return $"{{{key}, {value}}},";
        }

        private class CodeCompositions
        {
            public CodeCompositions(string constructionContext = null)
            {
                ConstructionContext = constructionContext ?? string.Empty;
            }

            public string ConstructionContext { get; }

            public bool HasConstructionContext => !string.IsNullOrWhiteSpace(ConstructionContext);

            public List<(string name, Type type, bool requiresContext)> ComposeMethods { get; } =
                new List<(string, Type, bool)>(32);

            public List<string> Methods { get; } = new List<string>(32);

            public List<string> Fields { get; } = new List<string>(32);

            public List<(string code, object value)> FieldInitializations { get; } = new List<(string, object)>(32);
        }
    }
}
