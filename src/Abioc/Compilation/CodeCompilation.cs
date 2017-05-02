// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Compilation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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
        private static readonly ConcurrentDictionary<string, Assembly> Compilations
            = new ConcurrentDictionary<string, Assembly>();

        /// <summary>
        /// Compiles the <paramref name="code"/> for the registration <paramref name="setup"/>.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <param name="code">The source code to compile.</param>
        /// <param name="fieldValues">The values of the fields to be initialized.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <returns>The <see cref="CompositionContext"/>.</returns>
        public static AbiocContainer<TExtra> Compile<TExtra>(
            RegistrationSetup<TExtra> setup,
            string code,
            object[] fieldValues,
            Assembly srcAssembly)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (fieldValues == null)
                throw new ArgumentNullException(nameof(fieldValues));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            ////Assembly assembly = Compilations.GetOrAdd(code, c => CompileCode(c, srcAssembly));
            Assembly assembly = CompileCode(code, srcAssembly);

            if (fieldValues.Length > 0)
            {
                var initializeFieldsMethod = GetInitializeFieldsMethodInfo(assembly);

                initializeFieldsMethod.Invoke(null, new object[] { fieldValues });
            }

            MethodInfo getCreateMapMethod = GetCreateMapMethodInfo(assembly);
            Dictionary<Type, Func<ConstructionContext<TExtra>, object>> createMap =
                (Dictionary<Type, Func<ConstructionContext<TExtra>, object>>)getCreateMapMethod.Invoke(null, null);

            Func<Type, ConstructionContext<TExtra>, object> getServiceMethod = GetGetServiceMethod<TExtra>(assembly);

            IEnumerable<(Type type, Func<ConstructionContext<TExtra>, object>[] compositions)> iocMappings =
                from kvp in setup.Registrations.Where(kvp => kvp.Value.Any(r => !r.Internal))
                let compositions = kvp.Value.Select(r => createMap[r.ImplementationType]).ToArray()
                select (kvp.Key, compositions);

            return iocMappings.ToDictionary(m => m.type, kvp => kvp.compositions).ToContainer(getServiceMethod);
        }

        /// <summary>
        /// Compiles the <paramref name="code"/> for the registration <paramref name="setup"/>.
        /// </summary>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <param name="code">The source code to compile.</param>
        /// <param name="fieldValues">The values of the fields to be initialized.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <returns>The <see cref="CompositionContext"/>.</returns>
        public static AbiocContainer Compile(
            RegistrationSetup setup,
            string code,
            object[] fieldValues,
            Assembly srcAssembly)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (fieldValues == null)
                throw new ArgumentNullException(nameof(fieldValues));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            ////Assembly assembly = Compilations.GetOrAdd(code, c => CompileCode(c, srcAssembly));
            Assembly assembly = CompileCode(code, srcAssembly);

            if (fieldValues.Length > 0)
            {
                var initializeFieldsMethod = GetInitializeFieldsMethodInfo(assembly);

                initializeFieldsMethod.Invoke(null, new object[] { fieldValues });
            }

            MethodInfo getCreateMapMethod = GetCreateMapMethodInfo(assembly);
            Dictionary<Type, Func<object>> createMap =
                (Dictionary<Type, Func<object>>)getCreateMapMethod.Invoke(null, null);

            Func<Type, object> getServiceMethod = GetGetServiceMethod(assembly);

            IEnumerable<(Type type, Func<object>[] compositions)> iocMappings =
                from kvp in setup.Registrations.Where(kvp => kvp.Value.Any(r => !r.Internal))
                let compositions = kvp.Value.Select(r => createMap[r.ImplementationType]).ToArray()
                select (kvp.Key, compositions);

            return iocMappings.ToDictionary(m => m.type, kvp => kvp.compositions).ToContainer(getServiceMethod);
        }

        private static MethodInfo GetCreateMapMethodInfo(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Type type = assembly.GetType("Abioc.Generated.Construction");

            MethodInfo getCreateMapMethod =
                type.GetTypeInfo().GetMethod("GetCreateMap", BindingFlags.NonPublic | BindingFlags.Static);

            return getCreateMapMethod;
        }

        private static Func<Type, ConstructionContext<TExtra>, object> GetGetServiceMethod<TExtra>(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Type type = assembly.GetType("Abioc.Generated.Construction");

            MethodInfo getGetServiceMethodInfo =
                type.GetTypeInfo().GetMethod("GetGetServiceMethod", BindingFlags.NonPublic | BindingFlags.Static);

            return (Func<Type, ConstructionContext<TExtra>, object>)getGetServiceMethodInfo.Invoke(null, null);
        }

        private static Func<Type, object> GetGetServiceMethod(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Type type = assembly.GetType("Abioc.Generated.Construction");

            MethodInfo getGetServiceMethodInfo =
                type.GetTypeInfo().GetMethod("GetGetServiceMethod", BindingFlags.NonPublic | BindingFlags.Static);

            return (Func<Type, object>)getGetServiceMethodInfo.Invoke(null, null);
        }

        private static MethodInfo GetInitializeFieldsMethodInfo(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            Type type = assembly.GetType("Abioc.Generated.Construction");

            MethodInfo getCreateMapMethod =
                type.GetTypeInfo().GetMethod("InitializeFields", BindingFlags.NonPublic | BindingFlags.Static);

            return getCreateMapMethod;
        }

        private static Assembly CompileCode(string code, Assembly srcAssembly)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            string tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, code, Encoding.UTF8);
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(code, path: tempFileName, encoding: Encoding.UTF8);
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
                .AddReferences(MetadataReference.CreateFromFile(typeof(CodeCompilation).GetTypeInfo().Assembly.Location))
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

                    throw new CompilationException(
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
    }
}
