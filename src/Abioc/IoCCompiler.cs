// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;

    /// <summary>
    /// Provides the IoC compilation functions.
    /// </summary>
    public static class IoCCompiler
    {
        private const string NamespaceDelimiter = "_ɸ_";

        /// <summary>
        /// Compiles the <paramref name="registration"/> context into the returned service function mappings.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
        /// <param name="registration">The registration context.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <returns>The compiled function mappings.</returns>
        public static CompilationContext<TContructionContext> Compile<TContructionContext>(
            this RegistrationContext<TContructionContext> registration,
            Assembly srcAssembly)
            where TContructionContext : IContructionContext
        {
            Assembly assembly = InternalCompile(registration, srcAssembly);
            Type type = assembly.GetType("I_ɸ_C.IoC_ɸ_Contruction");

            MethodInfo getCreateMapMethod =
                type.GetTypeInfo().GetMethod("GetCreateMap", BindingFlags.NonPublic | BindingFlags.Static);
            var createMap = (Dictionary<Type, Func<TContructionContext, object>>)getCreateMapMethod.Invoke(null, null);

            IEnumerable<KeyValuePair<Type, IReadOnlyList<Func<TContructionContext, object>>>> iocMappings =
                from kvp in registration.Context
                let createFuncs = kvp.Value.Select(v => createMap[v.ImplementationType]).ToArray()
                select new KeyValuePair<Type, IReadOnlyList<Func<TContructionContext, object>>>(kvp.Key, createFuncs);

            return iocMappings
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                .ToCompilationContext();
        }

        private static Assembly InternalCompile<TContructionContext>(
            this RegistrationContext<TContructionContext> registration,
            Assembly srcAssembly)
            where TContructionContext : IContructionContext
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            CompilationContext compilationContext = new CompilationContext();
            string contructionContext = typeof(TContructionContext).ToCompileName();

            // Start with all the implementations where there is a factory method.
            IReadOnlyList<RegistrationEntry<TContructionContext>> factoredTypes =
                registration.Context.Values
                    .SelectMany(entries => entries)
                    .Where(entry => entry.Factory != null)
                    .OrderBy(entry => entry.ImplementationType.FullName)
                    .ToList();

            IEnumerable<(string field, string initializer)> factoryInitialisers =
                GetFactoryInitialisers(factoredTypes, contructionContext);

            foreach ((string field, string initializer) factoryInitialiser in factoryInitialisers)
            {
                compilationContext.FactorFunctionFields.Add(factoryInitialiser.field);
                compilationContext.FactorFunctionInitializers.Add(factoryInitialiser.initializer);
            }

            // Now generate all the Create methods.
            IReadOnlyList<RegistrationEntry<TContructionContext>> createdTypes =
                registration.Context.Values
                    .SelectMany(entries => entries)
                    .Where(entry => entry.Factory == null)
                    .Where(entry => factoredTypes.All(fac => fac.ImplementationType != entry.ImplementationType))
                    .Concat(factoredTypes)
                    .DistinctBy(entry => entry.ImplementationType)
                    .OrderBy(entry => entry.ImplementationType.FullName)
                    .ToList();

            IEnumerable<string> createMethods = GetCreateMethods(
                registration,
                createdTypes.Where(entry => !entry.Typedfactory),
                contructionContext);
            compilationContext.CreateMethods.AddRange(createMethods);

            compilationContext.GetCreateMapMethod =
                GenerateGetCreateMapMethod(createdTypes.Select(f => f.ImplementationType), contructionContext);

            string code = GenerateCode(compilationContext, contructionContext);

            Assembly assembly = CompileCode(code, srcAssembly);

            Type type = assembly.GetType("I_ɸ_C.IoC_ɸ_Contruction");

            if (factoredTypes.Count > 0)
            {
                MethodInfo initialiseFactoryMethod =
                    type.GetTypeInfo()
                        .GetMethod("InitialiseFactoryFunctions", BindingFlags.NonPublic | BindingFlags.Static);

                initialiseFactoryMethod.Invoke(null, new object[] { factoredTypes.Select(f => f.Factory).ToList() });
            }

            return assembly;
        }

        private static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> items,
            Func<TSource, TKey> keySelector)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return items.GroupBy(keySelector).Select(item => item.First());
        }

        /// <summary>
        /// Gets the full name of a <paramref name="type"/> that is compilable as part of a method name, e.g. namespace
        /// and nested class delimiters <c>'.'</c> and <c>'+'</c> are replaced with valid characters for a method name.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to return the compilable method name part.</param>
        /// <returns>The full name of a <paramref name="type"/> that is compilable as part of a method name.</returns>
        private static string ToCompileMethodName(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            string name = Regex.Replace(type.FullName, @"[\.\+]", NamespaceDelimiter);
            return name;
        }

        /// <summary>
        /// Gets the full name of a <paramref name="type"/> that is compilable, e.g. nested classes have a <c>'+'</c>
        /// delimiter. This is replaced with a <c>'.'</c> to ensure compilation succeeds.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to return the compilable name.</param>
        /// <returns>The full name of a <paramref name="type"/> that is compilable.</returns>
        private static string ToCompileName(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            string name = type.FullName.Replace('+', '.');
            return name;
        }

        private static IEnumerable<(string field, string initializer)> GetFactoryInitialisers<TContructionContext>(
            IEnumerable<RegistrationEntry<TContructionContext>> factoredEntries,
            string contructionContext)
            where TContructionContext : IContructionContext
        {
            if (factoredEntries == null)
                throw new ArgumentNullException(nameof(factoredEntries));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));

            int index = 0;
            foreach (RegistrationEntry<TContructionContext> entry in factoredEntries)
            {
                if (entry.Typedfactory)
                {
                    string createFuncFieldName = "Create_" + entry.ImplementationType.ToCompileMethodName();
                    string createReturnType = entry.ImplementationType.ToCompileName();
                    string createFuncFieldType = $"System.Func<{contructionContext}, {createReturnType}>";

                    string createField = $"private static {createFuncFieldType} {createFuncFieldName};";
                    string createInitializer = $"{createFuncFieldName} = ({createFuncFieldType}) facs[{index++}];";
                    yield return (createField, createInitializer);
                }
                else
                {
                    string factoryFunFieldName = "Factor_" + entry.ImplementationType.ToCompileMethodName();
                    string factoryReturnType = "object";
                    string factoryFuncFieldType = $"System.Func<{contructionContext}, {factoryReturnType}>";

                    string factoryField = $"private static {factoryFuncFieldType} {factoryFunFieldName};";
                    string factoryInitializer = $"{factoryFunFieldName} = facs[{index++}];";
                    yield return (factoryField, factoryInitializer);
                }
            }
        }

        private static IEnumerable<string> GetCreateMethods<TContructionContext>(
            RegistrationContext<TContructionContext> registration,
            IEnumerable<RegistrationEntry<TContructionContext>> createdTypes,
            string contructionContext)
            where TContructionContext : IContructionContext
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (createdTypes == null)
                throw new ArgumentNullException(nameof(createdTypes));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));

            foreach (RegistrationEntry<TContructionContext> createdType in createdTypes)
            {
                string method = createdType.Factory != null
                    ? GenerateCreateFromWeaklyTypedFactoryMethod(createdType.ImplementationType, contructionContext)
                    : GenerateCreateNewMethod(registration, createdType.ImplementationType, contructionContext);
                yield return method;
            }
        }

        private static string GenerateGetCreateMapMethod(IEnumerable<Type> createdTypes, string contructionContext)
        {
            if (createdTypes == null)
                throw new ArgumentNullException(nameof(createdTypes));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));

            IEnumerable<string> initializers =
                createdTypes.Select(t => $"{{typeof({t.ToCompileName()}), Create_{t.ToCompileMethodName()}}},");

            string method = string.Format(
                @"
private static System.Collections.Generic.Dictionary<System.Type, System.Func<{0}, object>> GetCreateMap()
{{
    return new System.Collections.Generic.Dictionary<System.Type, System.Func<{0}, object>>
    {{
        {1}
    }};
}}",
                contructionContext,
                string.Join("\r\n        ", initializers));

            return method;
        }

        private static string GenerateCreateFromWeaklyTypedFactoryMethod(
            Type typeToCreate,
            string contructionContext)
        {
            if (typeToCreate == null)
                throw new ArgumentNullException(nameof(typeToCreate));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));

            string method = string.Format(
                @"
private static {0} Create_{1}(
    {2} context)
{{
    object obj = Factor_{1}(context);
    if (obj == null)
    {{
        throw new System.InvalidOperationException(""The factory method to create an instance of '{0}' returned null."");
    }}

    var instance = obj as {0};
    if (instance != null)
    {{
        return instance;
    }}

    string message = $""The factory method to create an instance of '{0}' returned an instance of '{{obj.GetType()}}'."";
    throw new System.InvalidOperationException(message);
}}",
                typeToCreate.ToCompileName(),
                typeToCreate.ToCompileMethodName(),
                contructionContext);

            return method;
        }

        private static string GenerateCreateNewMethod<TContructionContext>(
            RegistrationContext<TContructionContext> registration,
            Type typeToCreate,
            string contructionContext)
            where TContructionContext : IContructionContext
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (typeToCreate == null)
                throw new ArgumentNullException(nameof(typeToCreate));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));

            ConstructorInfo[] constructors =
                typeToCreate.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // There must be a public constructor.
            if (constructors.Length == 0)
            {
                string message = $"The service of type '{typeToCreate}' has no public constructors.";
                throw new IoCCompilationException(message);
            }

            // There must be just 1 public constructor.
            if (constructors.Length > 1)
            {
                string message = $"The service of type '{typeToCreate}' has {constructors.Length:N0} " +
                                 "public constructors. There must be just 1.";
                throw new IoCCompilationException(message);
            }

            ConstructorInfo constructorInfo = constructors[0];
            ParameterInfo[] parameters = constructorInfo.GetParameters();
            IEnumerable<Type> parameterTypes =
                parameters.Select(
                    p =>
                        registration.Context.Values
                            .SelectMany(f => f)
                            .FirstOrDefault(f => f.ImplementationType == p.ParameterType)
                            ?.ImplementationType
                        ?? registration.Context[p.ParameterType].Single().ImplementationType);

            IEnumerable<string> paramCalls =
                parameterTypes.Select(p => $"Create_{p.ToCompileMethodName()}(context)");
            string paramArgs = string.Join(",\r\n        ", paramCalls);

            string method = string.Format(
                @"
private static {0} Create_{1}(
    {2} context)
{{
    return new {0}(
        {3}
    );
}}",
                typeToCreate.ToCompileName(),
                typeToCreate.ToCompileMethodName(),
                contructionContext,
                paramArgs);

            return method;
        }

        private static string GenerateCode(CompilationContext compilationContext, string contructionContext)
        {
            if (compilationContext == null)
                throw new ArgumentNullException(nameof(compilationContext));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));

            var builder = new StringBuilder();
            builder.Append(@"namespace I_ɸ_C
{
    public static class IoC_ɸ_Contruction
    {
");

            GenerateFactoryInitialisers(builder, compilationContext, contructionContext);
            foreach (string cretaeMethod in compilationContext.CreateMethods)
            {
                builder.AppendLine(cretaeMethod.Replace("\r\n", "\r\n        "));
            }

            builder.AppendLine(compilationContext.GetCreateMapMethod.Replace("\r\n", "\r\n        "));

            builder.Append(@"
    }
}
");
            string code = builder.ToString();
            return code;
        }

        private static void GenerateFactoryInitialisers(
            StringBuilder builder,
            CompilationContext compilationContext,
            string contructionContext)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (compilationContext == null)
                throw new ArgumentNullException(nameof(compilationContext));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));

            if (compilationContext.FactorFunctionFields.Count <= 0)
                return;

            IEnumerable<string> fields = compilationContext.FactorFunctionFields.Select(f => "        " + f);
            IEnumerable<string> initializers =
                compilationContext.FactorFunctionInitializers.Select(f => "            " + f);

            builder.Append(string.Join(Environment.NewLine, fields));

            builder.AppendFormat(
                @"

        private static void InitialiseFactoryFunctions(
            System.Collections.Generic.IReadOnlyList<System.Func<{0}, object>> facs)
        {{
", contructionContext);

            builder.Append(string.Join(Environment.NewLine, initializers));

            builder.Append(@"
        }");
        }

        // Workaround https://github.com/dotnet/roslyn/issues/12393#issuecomment-277933006
        // using this blog post
        // http://code.fitness/post/2017/02/using-csharpscript-with-netstandard.html
        private static string GetSystemAssemblyPathByName(string assemblyName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName));

            string root = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            string path = Path.Combine(root, assemblyName);
            return path;
        }

        private static Assembly CompileCode(string code, Assembly srcAssembly)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code);
            ProcessDiagnostics(tree);

            var options = new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release);

            // Create a compilation for the syntax tree
            var compilation = CSharpCompilation.Create("mylib.dll")
                .WithOptions(options)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(GetSystemAssemblyPathByName("System.Runtime.dll")))
                .AddReferences(MetadataReference.CreateFromFile(GetSystemAssemblyPathByName("mscorlib.dll")))
                .AddReferences(MetadataReference.CreateFromFile(typeof(IoCCompiler).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(srcAssembly.Location))
                .AddSyntaxTrees(tree);

            using (var stream = new MemoryStream())
            {
                EmitResult result = compilation.Emit(stream);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    string errors =
                        string.Join(Environment.NewLine, failures.Select(d => $"{d.Id}: {d.GetMessage()}"));

                    throw new InvalidOperationException(
                        $"Compilation failed.{Environment.NewLine}{errors}{Environment.NewLine}{code}");
                }

                stream.Seek(0, SeekOrigin.Begin);
#if NET46
                Assembly assembly = Assembly.Load(stream.ToArray());
#else
                Assembly assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
#endif
                return assembly;
            }
        }

        private static void ProcessDiagnostics(SyntaxTree tree)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            // detects diagnostics in the source code
            IReadOnlyList<Diagnostic> diagnostics = tree.GetDiagnostics().ToList();
            ProcessDiagnostics(diagnostics);
        }

        private static void ProcessDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            if (diagnostics == null)
                throw new ArgumentNullException(nameof(diagnostics));

            // TODO: Format this into something useful.
        }

        private class CompilationContext
        {
            public List<string> FactorFunctionFields { get; } = new List<string>();

            public List<string> FactorFunctionInitializers { get; } = new List<string>();

            public List<string> CreateMethods { get; } = new List<string>();

            public string GetCreateMapMethod { get; set; }
        }
    }
}
