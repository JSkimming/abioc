// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition;
    using Abioc.Registration;

    /// <summary>
    /// Wraps a <see cref="GenerationContext"/> with providing a specific instance of a
    /// <see cref="ConstructionContextDefinition"/>.
    /// </summary>
    internal class GenerationContextWrapper : GenerationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationContextWrapper"/> class.
        /// </summary>
        /// <param name="registrations">The setup <see cref="GenerationContext.Registrations"/>.</param>
        /// <param name="compositions">The <see cref="GenerationContext.Compositions"/> for code generation.</param>
        /// <param name="usingSimpleNames">
        /// A value indicating whether simple names should be generated for compose methods.
        /// </param>
        /// <param name="extraDataType">The type of the <see cref="ConstructionContext{T}.Extra"/> data.</param>
        /// <param name="constructionContext">The type of the <see cref="ConstructionContext{T}"/>.</param>
        public GenerationContextWrapper(
            IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations,
            IReadOnlyDictionary<Type, IComposition> compositions,
            bool usingSimpleNames,
            string extraDataType = null,
            string constructionContext = null)
            : base(registrations, compositions, usingSimpleNames, extraDataType, constructionContext)
        {
            ConstructionContextDefinition =
                new ConstructionContextDefinition(typeof(void), typeof(void), typeof(void));
            Inner = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationContextWrapper"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="GenerationContext"/> wrapped by this instance.</param>
        /// <param name="constructionContextDefinition">The <see cref="ConstructionContextDefinition"/></param>
        private GenerationContextWrapper(
            GenerationContext inner,
            ConstructionContextDefinition constructionContextDefinition)
            : base(inner.Registrations, inner.Compositions, inner.UsingSimpleNames, inner.ExtraDataType, inner.ConstructionContext)
        {
            if (constructionContextDefinition == null)
                throw new ArgumentNullException(nameof(constructionContextDefinition));

            Inner = inner;
            ConstructionContextDefinition = constructionContextDefinition;
        }

        /// <summary>
        /// Gets the inner <see cref="GenerationContext"/> wrapped by this instance.
        /// </summary>
        public GenerationContext Inner { get; }

        /// <inheritdoc/>
        public override ConstructionContextDefinition ConstructionContextDefinition { get; }

        /// <inheritdoc/>
        public override IGenerationContext Customize(ConstructionContextDefinition constructionContextDefinition)
        {
            if (constructionContextDefinition == null)
                throw new ArgumentNullException(nameof(constructionContextDefinition));

            var wrapper = new GenerationContextWrapper(Inner, constructionContextDefinition);
            return wrapper;
        }
    }
}
