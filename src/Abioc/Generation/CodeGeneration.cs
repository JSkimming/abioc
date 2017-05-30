// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Abioc.Composition;

    /// <summary>
    /// Generates the code from a <see cref="CompositionContainer"/>.
    /// </summary>
    public static class CodeGeneration
    {
        private static readonly object[] EmptyFieldValues = { };

        private static readonly string NewLine = Environment.NewLine;

        private static readonly string DoubleNewLine = NewLine + NewLine;

        /// <summary>
        /// Generates the code from the composition <paramref name="container"/>.
        /// </summary>
        /// <param name="container">The <see cref="CompositionContainer"/>.</param>
        /// <returns>The generated code from the composition <paramref name="container"/>.</returns>
        public static (string generatedCode, object[] fieldValues) GenerateCode(this CompositionContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            // First try with simple method names.
            GenerationContext context = new GenerationContextWrapper(
                registrations: container.Registrations,
                compositions: container.Compositions,
                usingSimpleNames: true,
                extraDataType: container.ExtraDataType?.ToCompileName(),
                constructionContext: container.ConstructionContextType?.ToCompileName());
            ProcessCompositions(context);

            // Check if there are any name conflicts.
            if (context.ComposeMethodsNames.Select(c => c.name).Distinct().Count() != context.ComposeMethodsNames.Count)
            {
                context = new GenerationContextWrapper(
                    registrations: container.Registrations,
                    compositions: container.Compositions,
                    usingSimpleNames: false,
                    extraDataType: container.ExtraDataType?.ToCompileName(),
                    constructionContext: container.ConstructionContextType?.ToCompileName());
                ProcessCompositions(context);
            }

            string generatedCode = GenerateCode(context);
            object[] fieldValues =
                context.FieldInitializations.Count == 0
                    ? EmptyFieldValues
                    : context.FieldInitializations.Select(fi => fi.value).ToArray();

            return (generatedCode, fieldValues);
        }

        private static void ProcessCompositions(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Some compositions are keyed under different multiple types, e.g. an interface mapped to a class,
            // therefore only process the distinct compositions.
            IEnumerable<IComposition> compositions =
                context.Compositions
                    .Values
                    .Distinct()
                    .OrderBy(r => r.Type.ToCompileName());

            foreach (IComposition composition in compositions)
            {
                Type type = composition.Type;
                string composeMethodName = composition.GetComposeMethodName(context);
                bool requiresConstructionContext = composition.RequiresConstructionContext(context);

                context.AddComposeMethodsName(composeMethodName, type, requiresConstructionContext);
                IEnumerable<string> methods = composition.GetMethods(context);
                foreach (string method in methods)
                {
                    context.AddMethod(method);
                }

                IEnumerable<string> fields = composition.GetFields(context);
                foreach (string field in fields)
                {
                    context.AddField(field);
                }

                IEnumerable<(string snippet, object value)> fieldInitializations =
                    composition.GetFieldInitializations(context);
                foreach ((string snippet, object value) in fieldInitializations)
                {
                    context.AddFieldInitialization(snippet, value);
                }

                IEnumerable<string> additionalInitializations =
                    composition.GetAdditionalInitializations(context);
                foreach (string additionalInitialization in additionalInitializations)
                {
                    context.AddAdditionalInitialization(additionalInitialization);
                }
            }
        }

        private static string GenerateCode(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string genericContainerParam = context.HasConstructionContext ? $"<{context.ExtraDataType}>" : string.Empty;

            var builder = new StringBuilder(10240);
            builder.AppendFormat(
                "namespace Abioc.Generated{0}{{{0}    using System.Linq;{0}{0}    internal class Container : " +
                "Abioc.IContainerInitialization{1}, Abioc.IContainer{1}{0}    {{",
                NewLine,
                genericContainerParam);

            string fieldsAndMethods = GenerateFieldsAndMethods(context);
            fieldsAndMethods = CodeGen.Indent(NewLine + fieldsAndMethods, 2);
            builder.Append(fieldsAndMethods);

            builder.Append(NewLine);
            string fieldInitializationsMethod = GenerateConstructor(context);
            fieldInitializationsMethod = CodeGen.Indent(NewLine + fieldInitializationsMethod, 2);
            builder.Append(fieldInitializationsMethod);

            builder.Append(NewLine);
            string composeMapMethod = GenerateComposeMapMethod(context);
            composeMapMethod = CodeGen.Indent(NewLine + composeMapMethod, 2);
            builder.Append(composeMapMethod);

            builder.Append(NewLine);
            string getServiceMethod = GenerateGetServiceMethod(context);
            getServiceMethod = CodeGen.Indent(NewLine + getServiceMethod, 2);
            builder.Append(getServiceMethod);

            builder.Append(NewLine);
            string getServicesMethod = GenerateGetServicesMethod(context);
            getServicesMethod = CodeGen.Indent(NewLine + getServicesMethod, 2);
            builder.Append(getServicesMethod);

            builder.AppendFormat("{0}    }}{0}}}{0}", NewLine);

            var generatedCode = builder.ToString();
            return generatedCode;
        }

        private static string GenerateFieldsAndMethods(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string fields = string.Join(NewLine, context.Fields);
            string methods = string.Join(DoubleNewLine, context.Methods);
            string fieldsAndMethods = fields;
            if (fieldsAndMethods.Length > 0)
                fieldsAndMethods += DoubleNewLine;
            fieldsAndMethods = fieldsAndMethods + methods;

            return fieldsAndMethods;
        }

        private static string GenerateConstructor(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public Container(" +
                "{0}    object[] fieldValues){0}{{",
                NewLine);

            for (int index = 0; index < context.FieldInitializations.Count; index++)
            {
                (string snippet, object value) = context.FieldInitializations[index];
                builder.Append($"{NewLine}    {snippet}fieldValues[{index}];");
            }

            foreach (string additionalInitialization in context.AdditionalInitializations)
            {
                builder.Append(CodeGen.Indent(NewLine + additionalInitialization));
            }

            builder.AppendFormat("{0}}}", NewLine);
            return builder.ToString();
        }

        private static string GenerateComposeMapMethod(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string composeMapType = context.HasConstructionContext
                ? $"System.Collections.Generic.Dictionary<System.Type, System.Func<{context.ConstructionContext}, object>>"
                : "System.Collections.Generic.Dictionary<System.Type, System.Func<object>>";

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public {0} GetCreateMap(){1}{{{1}    return new {0}{1}    {{",
                composeMapType,
                NewLine);

            string initializers =
                string.Join(
                    NewLine,
                    context.ComposeMethodsNames.Select(c => GenerateComposeMapInitializer(context.HasConstructionContext, c)));
            initializers = CodeGen.Indent(NewLine + initializers, 2);
            builder.Append(initializers);

            builder.AppendFormat("{0}    }};{0}}}", NewLine);
            return builder.ToString();
        }

        private static string GenerateGetServiceMethod(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string parameter = context.HasConstructionContext
                ? $",{NewLine}    {context.ExtraDataType} extraData"
                : string.Empty;

            string contextVariable = context.HasConstructionContext
                ? $"{NewLine}    var context = {context.ConstructionContext}.Default.Initialize(this, extraData);"
                : string.Empty;

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public object GetService({0}    System.Type serviceType{1}){0}{{{2}{0}    " +
                "switch (serviceType.GetHashCode()){0}    {{",
                NewLine,
                parameter,
                contextVariable);

            IEnumerable<(Type key, IComposition composition)> singleIocMappings =
                from kvp in context.Registrations
                where kvp.Value.Count(r => !r.Internal) == 1
                orderby kvp.Key.GetHashCode()
                select (kvp.Key, context.Compositions[kvp.Value.Single(r => !r.Internal).ImplementationType]);

            IEnumerable<string> caseSnippets = singleIocMappings.Select(m => GetCaseSnippet(m.key, m.composition));
            string caseStatements = string.Join(NewLine, caseSnippets);
            caseStatements = CodeGen.Indent(NewLine + caseStatements, 2);
            builder.Append(caseStatements);

            string GetCaseSnippet(Type key, IComposition composition)
            {
                var definition = new ConstructionContextDefinition(composition.Type, key, typeof(void));
                string keyComment = key.ToCompileName();
                string instanceExpression = composition.GetInstanceExpression(context.Customize(definition));
                instanceExpression = CodeGen.Indent(instanceExpression);

                string caseSnippet =
                    $"case {key.GetHashCode()}: // {keyComment}{NewLine}    return {instanceExpression};";
                return caseSnippet;
            }

            builder.AppendFormat("{0}    }}{0}{0}    return null;{0}}}", NewLine);

            return builder.ToString();
        }

        private static string GenerateGetServicesMethod(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string parameter = context.HasConstructionContext
                ? $",{NewLine}    {context.ExtraDataType} extraData"
                : string.Empty;

            string contextVariable = context.HasConstructionContext
                ? $"{NewLine}    var context = {context.ConstructionContext}.Default.Initialize(this, extraData);"
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
                from kvp in context.Registrations
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
                    from composition in compositions
                    let definition = new ConstructionContextDefinition(composition.Type, key, typeof(void))
                    let snippet = composition.GetInstanceExpression(context.Customize(definition))
                    let instanceExpression = $"{NewLine}yield return {snippet};"
                    select CodeGen.Indent(instanceExpression);

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
    }
}
