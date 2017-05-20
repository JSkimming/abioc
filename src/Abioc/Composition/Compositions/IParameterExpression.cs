// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;

    /// <summary>
    /// Defines the interface a parameter expression must implement.
    /// </summary>
    internal interface IParameterExpression
    {
        /// <summary>
        /// Gets the code for the expression (e.g. method call or instance field) to retrieve an instance of the
        /// <see cref="Type"/>.
        /// </summary>
        /// <param name="container">The whole composition context.</param>
        /// <param name="simpleName">
        /// <p>
        /// If <see langword="true"/> simple method names should be produced; otherwise produce complex method names.
        /// </p>
        /// <p>
        /// A simple method may potentially produce conflicts; though it will produce more readable code.
        /// </p>
        /// </param>
        /// <returns>
        /// The code for the expression (e.g. method call or instance field) to retrieve an instance of the
        /// <see cref="Type"/>.
        /// </returns>
        string GetInstanceExpression(CompositionContainer container, bool simpleName);

        /// <summary>
        /// Returns the value indicating whether the <see cref="IParameterExpression"/> requires a
        /// <see cref="ConstructionContext{T}"/>.
        /// </summary>
        /// <param name="container">The whole composition context.</param>
        /// <returns>
        /// The value indicating whether the <see cref="IParameterExpression"/> requires a
        /// <see cref="ConstructionContext{T}"/>.
        /// </returns>
        bool RequiresConstructionContext(CompositionContainer container);
    }
}
