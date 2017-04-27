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
        /// <typeparam name="TConstructionContext">The type of the construction context.</typeparam>
        /// <param name="registration">The registration context.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <returns>The compiled function mappings.</returns>
        public static CompilationContext<TConstructionContext> Compile<TConstructionContext>(
            this RegistrationContext<TConstructionContext> registration,
            Assembly srcAssembly)
            where TConstructionContext : IConstructionContext
        {
            Assembly assembly = InternalCompile<TConstructionContext>(registration, srcAssembly);
            Type type = assembly.GetType("I_ɸ_C.IoC_ɸ_Construction");

            MethodInfo getCreateMapMethod =
                type.GetTypeInfo().GetMethod("GetCreateMap", BindingFlags.NonPublic | BindingFlags.Static);
            var createMap = (Dictionary<Type, Func<TConstructionContext, object>>)getCreateMapMethod.Invoke(null, null);

            IEnumerable<KeyValuePair<Type, IReadOnlyList<Func<TConstructionContext, object>>>> iocMappings =
                from kvp in registration.Context
                let createFuncs = kvp.Value.Select(v => createMap[v.ImplementationType]).ToArray()
                select new KeyValuePair<Type, IReadOnlyList<Func<TConstructionContext, object>>>(kvp.Key, createFuncs);

            return iocMappings
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                .ToCompilationContext();
        }

        private static Assembly InternalCompile<TConstructionContext>(
            this RegistrationContextBase registration,
            Assembly srcAssembly)
            where TConstructionContext : IConstructionContext
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            CompilationContext compilationContext = new CompilationContext();
            string constructionContext = typeof(TConstructionContext).ToCompileName();

            // Start with all the implementations where there is a factory method.
            IReadOnlyList<RegistrationEntry> factoredTypes =
                registration.Context.Values
                    .SelectMany(entries => entries)
                    .Where(entry => entry.Factory != null)
                    .OrderBy(entry => entry.ImplementationType.FullName)
                    .ToList();

            IEnumerable<(string field, string initializer)> factoryInitialisers =
                GetFactoryInitialisers(factoredTypes, constructionContext);

            foreach ((string field, string initializer) factoryInitialiser in factoryInitialisers)
            {
                compilationContext.FactorFunctionFields.Add(factoryInitialiser.field);
                compilationContext.FactorFunctionInitializers.Add(factoryInitialiser.initializer);
            }

            // Now generate all the Create methods.
            IReadOnlyList<RegistrationEntry> createdTypes =
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
                createdTypes.Where(entry => !entry.TypedFactory),
                constructionContext);
            compilationContext.CreateMethods.AddRange(createMethods);

            compilationContext.GetCreateMapMethod =
                GenerateGetCreateMapMethod(createdTypes, constructionContext);

            string code = GenerateCode(compilationContext, constructionContext);

            Assembly assembly = CompileCode(code, srcAssembly);

            Type type = assembly.GetType("I_ɸ_C.IoC_ɸ_Construction");

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

        private static IEnumerable<(string field, string initializer)> GetFactoryInitialisers(
            IEnumerable<RegistrationEntry> factoredEntries,
            string constructionContext)
        {
            if (factoredEntries == null)
                throw new ArgumentNullException(nameof(factoredEntries));
            if (constructionContext == null)
                throw new ArgumentNullException(nameof(constructionContext));

            int index = 0;
            foreach (RegistrationEntry entry in factoredEntries)
            {
                if (entry.TypedFactory)
                {
                    string createFuncFieldName = "Create_" + entry.ImplementationType.ToCompileMethodName();
                    string createReturnType = entry.ImplementationType.ToCompileName();
                    string createFuncFieldType =
                        entry.FactoryRequiresContext
                            ? $"System.Func<{constructionContext}, {createReturnType}>"
                            : $"System.Func<{createReturnType}>";

                    string createField = $"private static {createFuncFieldType} {createFuncFieldName};";
                    string createInitializer = $"{createFuncFieldName} = ({createFuncFieldType})facs[{index++}];";
                    yield return (createField, createInitializer);
                }
                else
                {
                    string factoryFunFieldName = "Factor_" + entry.ImplementationType.ToCompileMethodName();
                    string factoryReturnType = "object";
                    string factoryFuncFieldType =
                        entry.FactoryRequiresContext
                            ? $"System.Func<{constructionContext}, {factoryReturnType}>"
                            : $"System.Func<{factoryReturnType}>";

                    string factoryField = $"private static {factoryFuncFieldType} {factoryFunFieldName};";
                    string factoryInitializer = $"{factoryFunFieldName} = ({factoryFuncFieldType})facs[{index++}];";
                    yield return (factoryField, factoryInitializer);
                }
            }
        }

        private static IEnumerable<string> GetCreateMethods(
            RegistrationContextBase registration,
            IEnumerable<RegistrationEntry> createdTypes,
            string constructionContext)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (createdTypes == null)
                throw new ArgumentNullException(nameof(createdTypes));
            if (constructionContext == null)
                throw new ArgumentNullException(nameof(constructionContext));

            foreach (RegistrationEntry createdType in createdTypes)
            {
                string method = createdType.Factory != null
                    ? GenerateCreateFromWeaklyTypedFactoryMethod(createdType, constructionContext)
                    : GenerateCreateNewMethod(registration, createdType.ImplementationType, constructionContext);
                yield return method;
            }
        }

        private static string GenerateGetCreateMapMethod(IEnumerable<RegistrationEntry> createdTypes, string constructionContext)
        {
            if (createdTypes == null)
                throw new ArgumentNullException(nameof(createdTypes));
            if (constructionContext == null)
                throw new ArgumentNullException(nameof(constructionContext));

            IEnumerable<string> initializers = createdTypes.Select(GetInitializer);

            string method = string.Format(
                @"
private static System.Collections.Generic.Dictionary<System.Type, System.Func<{0}, object>> GetCreateMap()
{{
    return new System.Collections.Generic.Dictionary<System.Type, System.Func<{0}, object>>
    {{
        {1}
    }};
}}",
                constructionContext,
                string.Join("\r\n        ", initializers));

            string GetInitializer(RegistrationEntry entry)
            {
                string key = $"typeof({entry.ImplementationType.ToCompileName()})";
                string createMethod = "Create_" + entry.ImplementationType.ToCompileMethodName();
                string value =
                    entry.Factory == null || entry.FactoryRequiresContext
                        ? createMethod
                        : $"c => {createMethod}()";

                return $"{{{key}, {value}}},";
            }

            return method;
        }

        private static string GenerateCreateFromWeaklyTypedFactoryMethod(
            RegistrationEntry typeToCreate,
            string constructionContext)
        {
            if (typeToCreate == null)
                throw new ArgumentNullException(nameof(typeToCreate));
            if (constructionContext == null)
                throw new ArgumentNullException(nameof(constructionContext));

            string factoredType = typeToCreate.ImplementationType.ToCompileName();
            string compileMethodName = typeToCreate.ImplementationType.ToCompileMethodName();

            string methodSignature =
                typeToCreate.FactoryRequiresContext
                    ? string.Format(
                        @"private static {0} Create_{1}(
    {2} context)",
                        factoredType,
                        compileMethodName,
                        constructionContext)
                    : $"private static {factoredType} Create_{compileMethodName}()";

            string factoryCall =
                typeToCreate.FactoryRequiresContext
                    ? $"Factor_{compileMethodName}(context)"
                    : $"Factor_{compileMethodName}()";

            string method = string.Format(
                @"
{0}
{{
    object obj = {1};
    if (obj == null)
    {{
        throw new System.InvalidOperationException(""The factory method to create an instance of '{2}' returned null."");
    }}

    var instance = obj as {2};
    if (instance != null)
    {{
        return instance;
    }}

    string message = $""The factory method to create an instance of '{2}' returned an instance of '{{obj.GetType()}}'."";
    throw new System.InvalidOperationException(message);
}}",
                methodSignature,
                factoryCall,
                factoredType);

            return method;
        }

        private static string GenerateCreateNewMethod(
            RegistrationContextBase registration,
            Type typeToCreate,
            string constructionContext)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (typeToCreate == null)
                throw new ArgumentNullException(nameof(typeToCreate));
            if (constructionContext == null)
                throw new ArgumentNullException(nameof(constructionContext));

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
            IEnumerable<RegistrationEntry> parameterTypes = parameters.Select(GetEntryForParameter);

            RegistrationEntry GetEntryForParameter(ParameterInfo p)
            {
                // First see of there's a direct entry for the parameter type.
                RegistrationEntry entry =
                    registration.Context.Values
                           .SelectMany(f => f)
                           .FirstOrDefault(f => f.ImplementationType == p.ParameterType);

                // If it's not a direct entry, it must be a mapped type.
                entry = entry ?? registration.Context[p.ParameterType].Single();
                return entry;
            }

            IEnumerable<string> parameterMethodCalls = parameterTypes.Select(GetParameterMethodCall);

            string GetParameterMethodCall(RegistrationEntry entry)
            {
                string methodCall = "Create_" + entry.ImplementationType.ToCompileMethodName();
                methodCall += entry.Factory == null || entry.FactoryRequiresContext ? "(context)" : "()";
                return methodCall;
            }

            string paramArgs = string.Join(",\r\n        ", parameterMethodCalls);

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
                constructionContext,
                paramArgs);

            return method;
        }

        private static string GenerateCode(CompilationContext compilationContext, string constructionContext)
        {
            if (compilationContext == null)
                throw new ArgumentNullException(nameof(compilationContext));
            if (constructionContext == null)
                throw new ArgumentNullException(nameof(constructionContext));

            var builder = new StringBuilder();
            builder.Append(@"namespace I_ɸ_C
{
    public static class IoC_ɸ_Construction
    {
");

            GenerateFactoryInitialisers(builder, compilationContext);
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
            CompilationContext compilationContext)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (compilationContext == null)
                throw new ArgumentNullException(nameof(compilationContext));

            if (compilationContext.FactorFunctionFields.Count <= 0)
                return;

            IEnumerable<string> fields = compilationContext.FactorFunctionFields.Select(f => "        " + f);
            IEnumerable<string> initializers =
                compilationContext.FactorFunctionInitializers.Select(f => "            " + f);

            builder.Append(string.Join(Environment.NewLine, fields));

            builder.Append(
                @"

        private static void InitialiseFactoryFunctions(
            System.Collections.Generic.IReadOnlyList<object> facs)
        {
");

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
            var compilation = CSharpCompilation.Create($"{Guid.NewGuid():N}.dll")
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
