// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Generation
{
    using System;
    using System.Collections.Generic;
    using Abioc.Composition;

    /// <summary>
    /// The context for generated code snippets.
    /// </summary>
    public interface IGenerationContext
    {
        /// <summary>
        /// Gets the compositions for code generation.
        /// </summary>
        IReadOnlyDictionary<Type, IComposition> Compositions { get; }

        /// <summary>
        /// Gets the type of the <see cref="ConstructionContext{TExtra}"/>; otherwise <see langword="null"/> if there
        /// is no <see cref="ConstructionContext{TExtra}"/>.
        /// </summary>
        string ConstructionContext { get; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="ConstructionContext{TExtra}"/> is required for service
        /// resolution.
        /// </summary>
        bool HasConstructionContext { get; }

        /// <summary>
        /// Gets a value indicating whether simple names should be generated for compose methods. Simple names may not
        /// be unique, and if not, complex names will be generated.
        /// </summary>
        bool UsingSimpleNames { get; }

        /// <summary>
        /// Gets the definition for the requirements of a <see cref="ConstructionContext{T}"/>.
        /// </summary>
        ConstructionContextDefinition ConstructionContextDefinition { get; }

        /// <summary>
        /// Creates a customized <see cref="IGenerationContext"/> with the specific
        /// <paramref name="constructionContextDefinition"/>.
        /// </summary>
        /// <param name="constructionContextDefinition">
        /// The specific <see cref="ConstructionContextDefinition"/>.
        /// </param>
        /// <returns>
        /// A customized <see cref="IGenerationContext"/> with the specific
        /// <paramref name="constructionContextDefinition"/>.
        /// </returns>
        IGenerationContext Customize(ConstructionContextDefinition constructionContextDefinition);
    }
}
