// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Compilation;
    using Abioc.Composition;
    using Abioc.Registration;

    /// <summary>
    /// Helper methods to construct a <see cref="AbiocContainer"/> and <see cref="AbiocContainer{TExtra}"/> from a
    /// <see cref="RegistrationSetup"/> and <see cref="RegistrationSetup{TExtra}"/>.
    /// </summary>
    public static class ContainerConstruction
    {
        /// <summary>
        /// Constructs an <see cref="AbiocContainer"/> from the registration <paramref name="setup"/>.
        /// </summary>
        /// <param name="setup">The <see cref="RegistrationSetup"/>.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <returns>
        /// A <see cref="AbiocContainer"/> constructed from the registration <paramref name="setup"/>.
        /// </returns>
        public static AbiocContainer Construct(this RegistrationSetup setup, Assembly srcAssembly)
        {
            return setup.Construct(srcAssembly, out var unused);
        }

        /// <summary>
        /// Constructs an <see cref="AbiocContainer"/> from the registration <paramref name="setup"/>.
        /// </summary>
        /// <param name="setup">The <see cref="RegistrationSetup"/>.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <param name="code">The generated source code.</param>
        /// <returns>
        /// A <see cref="AbiocContainer"/> constructed from the registration <paramref name="setup"/>.
        /// </returns>
        public static AbiocContainer Construct(this RegistrationSetup setup, Assembly srcAssembly, out string code)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            code = setup.Compose().GenerateCode();
            AbiocContainer container = CodeCompilation.Compile(setup, code, srcAssembly);
            return container;
        }

        /// <summary>
        /// Constructs an <see cref="AbiocContainer"/> from the registration <paramref name="setup"/>.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ContructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The <see cref="RegistrationSetup"/>.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <returns>
        /// A <see cref="AbiocContainer"/> constructed from the registration <paramref name="setup"/>.
        /// </returns>
        public static AbiocContainer<TExtra> Construct<TExtra>(
            this RegistrationSetup<TExtra> setup,
            Assembly srcAssembly)
        {
            return setup.Construct(srcAssembly, out var unused);
        }

        /// <summary>
        /// Constructs an <see cref="AbiocContainer"/> from the registration <paramref name="setup"/>.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ContructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The <see cref="RegistrationSetup"/>.</param>
        /// <param name="srcAssembly">The source assembly for the types top create.</param>
        /// <param name="code">The generated source code.</param>
        /// <returns>
        /// A <see cref="AbiocContainer"/> constructed from the registration <paramref name="setup"/>.
        /// </returns>
        public static AbiocContainer<TExtra> Construct<TExtra>(
            this RegistrationSetup<TExtra> setup,
            Assembly srcAssembly,
            out string code)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (srcAssembly == null)
                throw new ArgumentNullException(nameof(srcAssembly));

            code = setup.Compose().GenerateCode();
            AbiocContainer<TExtra> container = CodeCompilation.Compile(setup, code, srcAssembly);
            return container;
        }
    }
}
