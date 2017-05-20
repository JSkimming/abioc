// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Abioc.Generation;
    using Abioc.Registration;

    /// <summary>
    /// Generates the code from a <see cref="CompositionContainer"/>.
    /// </summary>
    public static class CodeComposition
    {
        private static readonly object[] EmptyFieldValues = { };

        private static readonly string NewLine = Environment.NewLine;

        private static readonly string DoubleNewLine = NewLine + NewLine;

        /// <summary>
        /// Generates the code from the composition <paramref name="container"/>.
        /// </summary>
        /// <param name="container">The <see cref="CompositionContainer"/>.</param>
        /// <param name="registrations">The setup <see cref="RegistrationSetupBase{T}.Registrations"/>.</param>
        /// <returns>The generated code from the composition <paramref name="container"/>.</returns>
        public static (string generatedCode, object[] fieldValues) GenerateCode(
            this CompositionContainer container,
            IReadOnlyDictionary<Type, List<IRegistration>> registrations)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            return container.GenerateCode(registrations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
        }

        private static (string generatedCode, object[] fieldValues) GenerateCode(
            this CompositionContainer container,
            IReadOnlyDictionary<Type, IRegistration[]> registrations)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            // First try with simple method names.
            GenerationContext code = new GenerationContext(
                registrations: registrations,
                compositions: container.Compositions,
                usingSimpleNames: true);
            ProcessCompositions(code, container);

            // Check if there are any name conflicts.
            if (code.ComposeMethodsNames.Select(c => c.name).Distinct().Count() != code.ComposeMethodsNames.Count)
            {
                code = new GenerationContext(
                    registrations: registrations,
                    compositions: container.Compositions,
                    usingSimpleNames: false);
                ProcessCompositions(code, container);
            }

            string generatedCode = GenerateCode(container, code);
            object[] fieldValues =
                code.FieldInitializations.Count == 0
                    ? EmptyFieldValues
                    : code.FieldInitializations.Select(fi => fi.value).ToArray();

            return (generatedCode, fieldValues);
        }

        private static void ProcessCompositions(GenerationContext context, CompositionContainer container)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (container == null)
                throw new ArgumentNullException(nameof(container));

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
                string composeMethodName = composition.GetComposeMethodName(container, context.UsingSimpleNames);
                bool requiresConstructionContext = composition.RequiresConstructionContext(container);

                context.AddComposeMethodsName(composeMethodName, type, requiresConstructionContext);
                IEnumerable<string> methods = composition.GetMethods(container, context.UsingSimpleNames);
                foreach (string method in methods)
                {
                    context.AddMethod(method);
                }

                IEnumerable<string> fields = composition.GetFields(container);
                foreach (string field in fields)
                {
                    context.AddField(field);
                }

                IEnumerable<(string snippet, object value)> fieldInitializations =
                    composition.GetFieldInitializations(container);
                foreach ((string snippet, object value) in fieldInitializations)
                {
                    context.AddFieldInitialization(snippet, value);
                }

                IEnumerable<string> additionalInitializations =
                    composition.GetAdditionalInitializations(container, context.UsingSimpleNames);
                foreach (string additionalInitialization in additionalInitializations)
                {
                    context.AddAdditionalInitialization(additionalInitialization);
                }
            }
        }

        private static string GenerateCode(CompositionContainer container, GenerationContext code)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string genericContainerParam = container.HasConstructionContext ? $"<{container.ExtraDataType}>" : string.Empty;

            var builder = new StringBuilder(10240);
            builder.AppendFormat(
                "namespace Abioc.Generated{0}{{{0}    using System.Linq;{0}{0}    internal class Container : " +
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
            string composeMapMethod = GenerateComposeMapMethod(container, code);
            composeMapMethod = CodeGen.Indent(NewLine + composeMapMethod, 2);
            builder.Append(composeMapMethod);

            builder.Append(NewLine);
            string getServiceMethod = GenerateGetServiceMethod(container, code);
            getServiceMethod = CodeGen.Indent(NewLine + getServiceMethod, 2);
            builder.Append(getServiceMethod);

            builder.Append(NewLine);
            string getServicesMethod = GenerateGetServicesMethod(container, code);
            getServicesMethod = CodeGen.Indent(NewLine + getServicesMethod, 2);
            builder.Append(getServicesMethod);

            builder.AppendFormat("{0}    }}{0}}}{0}", NewLine);

            var generatedCode = builder.ToString();
            return generatedCode;
        }

        private static string GenerateFieldsAndMethods(GenerationContext code)
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

        private static string GenerateConstructor(GenerationContext code)
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

            foreach (string additionalInitialization in code.AdditionalInitializations)
            {
                builder.Append(CodeGen.Indent(NewLine + additionalInitialization));
            }

            builder.AppendFormat("{0}}}", NewLine);
            return builder.ToString();
        }

        private static string GenerateComposeMapMethod(CompositionContainer container, GenerationContext code)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string composeMapType = container.HasConstructionContext
                ? $"System.Collections.Generic.Dictionary<System.Type, System.Func<{container.ConstructionContext}, object>>"
                : "System.Collections.Generic.Dictionary<System.Type, System.Func<object>>";

            var builder = new StringBuilder(1024);
            builder.AppendFormat(
                "public {0} GetCreateMap(){1}{{{1}    return new {0}{1}    {{",
                composeMapType,
                NewLine);

            string initializers =
                string.Join(
                    NewLine,
                    code.ComposeMethodsNames.Select(c => GenerateComposeMapInitializer(container.HasConstructionContext, c)));
            initializers = CodeGen.Indent(NewLine + initializers, 2);
            builder.Append(initializers);

            builder.AppendFormat("{0}    }};{0}}}", NewLine);
            return builder.ToString();
        }

        private static string GenerateGetServiceMethod(CompositionContainer container, GenerationContext code)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string parameter = container.HasConstructionContext
                ? $",{NewLine}    {container.ExtraDataType} extraData"
                : string.Empty;

            string contextVariable = container.HasConstructionContext
                ? $"{NewLine}    var context = {container.ConstructionContext}.Default.Initialize(extraData);"
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
                select (kvp.Key, container.Compositions[kvp.Value.Single(r => !r.Internal).ImplementationType]);

            IEnumerable<string> caseSnippets = singleIocMappings.Select(m => GetCaseSnippet(m.key, m.composition));
            string caseStatements = string.Join(NewLine, caseSnippets);
            caseStatements = CodeGen.Indent(NewLine + caseStatements, 2);
            builder.Append(caseStatements);

            string GetCaseSnippet(Type key, IComposition composition)
            {
                string keyComment = key.ToCompileName();
                string instanceExpression = composition.GetInstanceExpression(container, code.UsingSimpleNames);
                instanceExpression = CodeGen.Indent(instanceExpression);

                string caseSnippet =
                    $"case {key.GetHashCode()}: // {keyComment}{NewLine}    return {instanceExpression};";
                return caseSnippet;
            }

            builder.AppendFormat("{0}    }}{0}{0}    return null;{0}}}", NewLine);

            return builder.ToString();
        }

        private static string GenerateGetServicesMethod(CompositionContainer container, GenerationContext code)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            string parameter = container.HasConstructionContext
                ? $",{NewLine}    {container.ExtraDataType} extraData"
                : string.Empty;

            string contextVariable = container.HasConstructionContext
                ? $"{NewLine}    var context = {container.ConstructionContext}.Default.Initialize(extraData);"
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
                    .Select(r => container.Compositions[r.ImplementationType])
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
                        .Select(c => $"{NewLine}yield return {c.GetInstanceExpression(container, code.UsingSimpleNames)};")
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
    }
}
