// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Abioc.Registration;

    /// <summary>
    /// Generates the code from a <see cref="CompositionContext"/>.
    /// </summary>
    public static class CodeComposition
    {
        private static readonly object[] EmptyFieldValues = { };

        private static readonly string NewLine = Environment.NewLine;

        private static readonly string DoubleNewLine = NewLine + NewLine;

        /// <summary>
        /// Generates the code from the composition <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="CompositionContext"/>.</param>
        /// <param name="registrations">The setup <see cref="RegistrationSetupBase{T}.Registrations"/>.</param>
        /// <returns>The generated code from the composition <paramref name="context"/>.</returns>
        public static (string generatedCode, object[] fieldValues) GenerateCode(
            this CompositionContext context,
            IReadOnlyDictionary<Type, List<IRegistration>> registrations)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            return context.GenerateCode(registrations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
        }

        private static (string generatedCode, object[] fieldValues) GenerateCode(
            this CompositionContext context,
            IReadOnlyDictionary<Type, IRegistration[]> registrations)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            IReadOnlyList<IComposition> compositions =
                context.Compositions.Values.DistinctBy(r => r.Type).OrderBy(r => r.Type.ToCompileName()).ToList();

            var code = new CodeCompositions(registrations, context.ConstructionContext);

            // First try with simple method names.
            foreach (IComposition composition in compositions)
            {
                code.UsingSimpleNames = true;
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
                    code.UsingSimpleNames = false;
                    Type type = composition.Type;
                    string composeMethodName = composition.GetComposeMethodName(context, simpleName: false);
                    bool requiresConstructionContext = composition.RequiresConstructionContext(context);

                    code.ComposeMethods.Add((composeMethodName, type, requiresConstructionContext));
                    code.Methods.AddRange(composition.GetMethods(context, simpleName: false));
                }
            }

            string generatedCode = GenerateCode(context, code);
            object[] fieldValues =
                code.FieldInitializations.Count == 0
                    ? EmptyFieldValues
                    : code.FieldInitializations.Select(fi => fi.value).ToArray();

            return (generatedCode, fieldValues);
        }

        private static string GenerateCode(CompositionContext context, CodeCompositions code)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string genericContainerParam = code.HasConstructionContext ? $"<{context.ExtraDataType}>" : string.Empty;

            var builder = new StringBuilder(10240);
            builder.AppendFormat(
                "namespace Abioc.Generated{0}{{{0}    internal class Container : " +
                "Abioc.IContainerInitialization{1}, Abioc.IContainer{1}{0}    {{",
                NewLine,
                genericContainerParam);

            string fieldsAndMethods = GenerateFieldsAndMethods(code);
            fieldsAndMethods = CodeGen.Indent(NewLine + fieldsAndMethods, 2);
            builder.Append(fieldsAndMethods);

            builder.Append(NewLine);
            string fieldInitializationsMethod = GenerateConstructor(code);
            fieldInitializationsMethod = CodeGen.Indent(NewLine + fieldInitializationsMethod, 2);
            builder.Append(fieldInitializationsMethod);

            builder.Append(NewLine);
            string composeMapMethod = GenerateComposeMapMethod(code);
            composeMapMethod = CodeGen.Indent(NewLine + composeMapMethod, 2);
            builder.Append(composeMapMethod);

            builder.Append(NewLine);
            string getServiceMethod = GenerateGetServiceMethod(context, code);
            getServiceMethod = CodeGen.Indent(NewLine + getServiceMethod, 2);
            builder.Append(getServiceMethod);

            builder.Append(NewLine);
            string getServicesMethod = GenerateGetServicesMethod(context, code);
            getServicesMethod = CodeGen.Indent(NewLine + getServicesMethod, 2);
            builder.Append(getServicesMethod);

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

        private static string GenerateConstructor(CodeCompositions code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public Container(" +
                "{0}    object[] fieldValues){0}{{",
                NewLine);

            for (int index = 0; index < code.FieldInitializations.Count; index++)
            {
                (string snippet, object value) = code.FieldInitializations[index];
                builder.Append($"{NewLine}    {snippet}fieldValues[{index}];");
            }

            builder.AppendFormat("{0}}}", NewLine);
            return builder.ToString();
        }

        private static string GenerateComposeMapMethod(CodeCompositions code)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string composeMapType = code.HasConstructionContext
                ? $"System.Collections.Generic.Dictionary<System.Type, System.Func<{code.ConstructionContext}, object>>"
                : "System.Collections.Generic.Dictionary<System.Type, System.Func<object>>";

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public {0} GetCreateMap(){1}{{{1}    return new {0}{1}    {{",
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

        private static string GenerateGetServiceMethod(CompositionContext context, CodeCompositions code)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string parameter = code.HasConstructionContext
                ? $",{NewLine}    {context.ExtraDataType} extraData"
                : string.Empty;

            string contextVariable = code.HasConstructionContext
                ? $"{NewLine}    var context = new {code.ConstructionContext}(typeof(object), typeof(object), " +
                  $"typeof(object), extraData);"
                : string.Empty;

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public object GetService({0}    System.Type serviceType{1}){0}{{{2}{0}    " +
                "switch (serviceType.GetHashCode()){0}    {{",
                NewLine,
                parameter,
                contextVariable);

            IEnumerable<(Type key, IComposition composition)> singleIocMappings =
                from kvp in code.Registrations
                where kvp.Value.Count(r => !r.Internal) == 1
                orderby kvp.Key.GetHashCode()
                select (kvp.Key, context.Compositions[kvp.Value.Single(r => !r.Internal).ImplementationType]);

            IEnumerable<string> caseSnippets = singleIocMappings.Select(m => GetCaseSnippet(m.key, m.composition));
            string caseStatements = string.Join(NewLine, caseSnippets);
            caseStatements = CodeGen.Indent(NewLine + caseStatements, 2);
            builder.Append(caseStatements);

            string GetCaseSnippet(Type key, IComposition composition)
            {
                string keyComment = key.ToCompileName();
                string instanceExpression = composition.GetInstanceExpression(context, code.UsingSimpleNames);
                instanceExpression = CodeGen.Indent(instanceExpression, 1);

                string caseSnippet =
                    $"case {key.GetHashCode()}: // {keyComment}{NewLine}    return {instanceExpression};";
                return caseSnippet;
            }

            builder.AppendFormat("{0}    }}{0}{0}    return null;{0}}}", NewLine);

            return builder.ToString();
        }

        private static string GenerateGetServicesMethod(CompositionContext context, CodeCompositions code)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string parameter = code.HasConstructionContext
                ? $",{NewLine}    {context.ExtraDataType} extraData"
                : string.Empty;

            string contextVariable = code.HasConstructionContext
                ? $"{NewLine}    var context = new {code.ConstructionContext}(typeof(object), typeof(object), " +
                  $"typeof(object), extraData);"
                : string.Empty;

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public System.Collections.Generic.IEnumerable<object> GetServices(" +
                "{0}    System.Type serviceType{1})" +
                "{0}{{{2}{0}    switch (serviceType.GetHashCode()){0}    {{",
                NewLine,
                parameter,
                contextVariable);

            IEnumerable<(Type key, IEnumerable<IComposition> compositions)> singleIocMappings =
                from kvp in code.Registrations
                where kvp.Value.Any(r => !r.Internal)
                let compositions = kvp.Value.Where(r => !r.Internal)
                    .Select(r => context.Compositions[r.ImplementationType])
                orderby kvp.Key.GetHashCode()
                select (kvp.Key, compositions);

            IEnumerable<string> caseSnippets = singleIocMappings.Select(m => GetCaseSnippet(m.key, m.compositions));
            string caseStatements = string.Join(NewLine, caseSnippets);
            caseStatements = CodeGen.Indent(NewLine + caseStatements, 2);
            builder.Append(caseStatements);

            string GetCaseSnippet(Type key, IEnumerable<IComposition> compositions)
            {
                string keyComment = key.ToCompileName();

                IEnumerable<string> instanceExpressions =
                    compositions
                        .Select(c => $"{NewLine}yield return {c.GetInstanceExpression(context, code.UsingSimpleNames)};")
                        .Select(instanceExpression => CodeGen.Indent(instanceExpression));

                string yieldStatements = string.Join(string.Empty, instanceExpressions);

                string caseSnippet =
                    $"case {key.GetHashCode()}: // {keyComment}{yieldStatements}{NewLine}    break;";
                return caseSnippet;
            }

            builder.AppendFormat("{0}    }}{0}}}", NewLine);

            return builder.ToString();
        }

        private static string GenerateComposeMapInitializer(
            bool hasContext,
            (string name, Type type, bool requiresContext) data)
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
            public CodeCompositions(
                IReadOnlyDictionary<Type, IRegistration[]> registrations,
                string constructionContext = null)
            {
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                Registrations = registrations;
                ConstructionContext = constructionContext;
            }

            public IReadOnlyDictionary<Type, IRegistration[]> Registrations { get; }

            public string ConstructionContext { get; }

            public bool HasConstructionContext => !string.IsNullOrWhiteSpace(ConstructionContext);

            public List<(string name, Type type, bool requiresContext)> ComposeMethods { get; } =
                new List<(string, Type, bool)>(32);

            public List<string> Methods { get; } = new List<string>(32);

            public List<string> Fields { get; } = new List<string>(32);

            public List<(string snippet, object value)> FieldInitializations { get; } = new List<(string, object)>(32);

            public bool UsingSimpleNames { get; set; }
        }
    }
}
