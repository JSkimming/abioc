// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Compilation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using Abioc.Composition;
    using Abioc.Registration;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;

    /// <summary>
    /// Compiles generated code into a dynamic assembly.
    /// </summary>
    public static class CodeCompilation
    {
        private static readonly ConcurrentDictionary<string, Compilation> Compilations
            = new ConcurrentDictionary<string, Compilation>();

        /// <summary>
        /// Compiles the <paramref name="code"/> for the registration <paramref name="setup"/>.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <param name="code">The source code to compile.</param>
        /// <param name="fieldValues">The values of the fields to be initialized.</param>
        /// <param name="srcAssemblies">The source assemblies for the types top create.</param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static AbiocContainer<TExtra> Compile<TExtra>(
            RegistrationSetup<TExtra> setup,
            string code,
            object[] fieldValues,
            params Assembly[] srcAssemblies)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (fieldValues == null)
                throw new ArgumentNullException(nameof(fieldValues));
            if (srcAssemblies == null)
                throw new ArgumentNullException(nameof(srcAssemblies));

            Compilation compilation = Compilations.GetOrAdd(code, GetOrAdd);

            Compilation GetOrAdd(string c)
            {
                Assembly assembly = CompileCode(c, srcAssemblies);
                Type containerType = assembly.GetType("Abioc.Generated.Container");
                Func<object[], IContainer<TExtra>> containerFactory = CreateContainerFactory<TExtra>(containerType);
                return new Compilation(assembly, containerType, containerFactory);
            }

            var factory = (Func<object[], IContainer<TExtra>>)compilation.ContainerFactory;
            IContainer<TExtra> container = factory(fieldValues);
            IContainerInitialization<TExtra> initialization = (IContainerInitialization<TExtra>)container;

            Dictionary<Type, Func<ConstructionContext<TExtra>, object>> createMap = initialization.GetCreateMap();

            // Get all mappings, where there are distinct non internal registrations.
            IEnumerable<(Type type, Func<ConstructionContext<TExtra>, object>[] compositions)> iocMappings =
                from kvp in setup.Registrations
                let regTypes = kvp.Value.DistinctPublicRegistrationTypes().ToList()
                where regTypes.Count > 0
                select (kvp.Key, regTypes.Select(type => createMap[type]).ToArray());

            return iocMappings.ToDictionary(m => m.type, kvp => kvp.compositions).ToContainer(container);
        }

        /// <summary>
        /// Compiles the <paramref name="code"/> for the registration <paramref name="setup"/>.
        /// </summary>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <param name="code">The source code to compile.</param>
        /// <param name="fieldValues">The values of the fields to be initialized.</param>
        /// <param name="srcAssemblies">The source assemblies for the types top create.</param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static AbiocContainer Compile(
            RegistrationSetup setup,
            string code,
            object[] fieldValues,
            params Assembly[] srcAssemblies)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (fieldValues == null)
                throw new ArgumentNullException(nameof(fieldValues));
            if (srcAssemblies == null)
                throw new ArgumentNullException(nameof(srcAssemblies));

            Compilation compilation = Compilations.GetOrAdd(code, GetOrAdd);

            Compilation GetOrAdd(string c)
            {
                Assembly assembly = CompileCode(c, srcAssemblies);
                Type containerType = assembly.GetType("Abioc.Generated.Container");
                Func<object[], IContainer> containerFactory = CreateContainerFactory(containerType);
                return new Compilation(assembly, containerType, containerFactory);
            }

            var factory = (Func<object[], IContainer>)compilation.ContainerFactory;
            IContainer container = factory(fieldValues);
            IContainerInitialization initialization = (IContainerInitialization)container;

            Dictionary<Type, Func<object>> createMap = initialization.GetCreateMap();

            // Get all mappings, where there are distinct non internal registrations.
            IEnumerable<(Type type, Func<object>[] compositions)> iocMappings =
                from kvp in setup.Registrations
                let regTypes = kvp.Value.DistinctPublicRegistrationTypes().ToList()
                where regTypes.Count > 0
                select (kvp.Key, regTypes.Select(type => createMap[type]).ToArray());

            return iocMappings.ToDictionary(m => m.type, kvp => kvp.compositions).ToContainer(container);
        }

        private static Func<object[], IContainer<TExtra>> CreateContainerFactory<TExtra>(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ConstructorInfo constructorInfo = type.GetTypeInfo().GetConstructor(new[] { typeof(object[]) });
            ParameterExpression parameter = Expression.Parameter(typeof(object[]), "fieldValues");
            NewExpression expression = Expression.New(constructorInfo, parameter);
            Expression<Func<object[], IContainer<TExtra>>> lambda =
                Expression.Lambda<Func<object[], IContainer<TExtra>>>(expression, parameter);

            Func<object[], IContainer<TExtra>> factory = lambda.Compile();
            return factory;
        }

        private static Func<object[], IContainer> CreateContainerFactory(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ConstructorInfo constructorInfo = type.GetTypeInfo().GetConstructor(new[] { typeof(object[]) });
            ParameterExpression parameter = Expression.Parameter(typeof(object[]), "fieldValues");
            NewExpression expression = Expression.New(constructorInfo, parameter);
            Expression<Func<object[], IContainer>> lambda =
                Expression.Lambda<Func<object[], IContainer>>(expression, parameter);

            Func<object[], IContainer> factory = lambda.Compile();
            return factory;
        }

        private static Assembly CompileCode(string code, Assembly[] srcAssemblies)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (srcAssemblies == null)
                throw new ArgumentNullException(nameof(srcAssemblies));

            // Need to review the code gen and file saving later.
            ////string tempFileName = Path.GetTempFileName();
            ////File.WriteAllText(tempFileName, code, Encoding.UTF8);
            ////SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code, path: tempFileName, encoding: Encoding.UTF8);
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code, encoding: Encoding.UTF8);
            ProcessDiagnostics(tree);

            var options = new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release);

            string[] assemblyReferences =
            {
                typeof(object).GetTypeInfo().Assembly.Location,
                typeof(Enumerable).GetTypeInfo().Assembly.Location,
#if NETSTANDARD2_0
                GetSystemAssemblyPathByName("System.Collections.dll"),
                GetSystemAssemblyPathByName("System.Runtime.dll"),
                GetSystemAssemblyPathByName("netstandard.dll"),
#elif NETSTANDARD1_5
                GetSystemAssemblyPathByName("System.Collections.dll"),
                GetSystemAssemblyPathByName("System.Runtime.dll"),
                GetSystemAssemblyPathByName("mscorlib.dll"),
#endif
                typeof(CodeCompilation).GetTypeInfo().Assembly.Location,
            };

            IEnumerable<PortableExecutableReference> references =
                assemblyReferences.Concat(srcAssemblies.Select(a => a.Location))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .Select(s => MetadataReference.CreateFromFile(s));

            // Create a compilation for the syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                $"{Guid.NewGuid():N}.dll",
                new[] { tree },
                references,
                options);

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

                    throw new CompilationException(
                        $"Compilation failed.{Environment.NewLine}{errors}{Environment.NewLine}{code}");
                }

                stream.Seek(0, SeekOrigin.Begin);
#if NETSTANDARD1_5
                Assembly assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
#else
                Assembly assembly = Assembly.Load(stream.ToArray());
#endif
                return assembly;
            }
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

        private class Compilation
        {
            private readonly Assembly _assembly;

            private readonly Type _containerType;

            public Compilation(Assembly assembly, Type containerType, object containerFactory)
            {
                _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
                _containerType = containerType ?? throw new ArgumentNullException(nameof(containerType));
                ContainerFactory = containerFactory ?? throw new ArgumentNullException(nameof(containerFactory));
            }

            public object ContainerFactory { get; }
        }
    }
}
