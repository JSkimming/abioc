// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The interface defined for a composition.
    /// </summary>
    public interface IComposition
    {
        /// <summary>
        /// Gets the type of the <see cref="IComposition"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the code for the expression (e.g. method call or instance field) to retrieve an instance of the
        /// <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The whole composition context.</param>
        /// <returns>
        /// The code for the expression (e.g. method call or instance field) to retrieve an instance of the
        /// <see cref="Type"/>.
        /// </returns>
        string GetInstanceExpression(CompositionContext context);

        /// <summary>
        /// Returns the code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The whole composition context.</param>
        /// <returns>
        /// The code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </returns>
        IEnumerable<string> GetFields(CompositionContext context);

        /// <summary>
        /// Returns the code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The whole composition context.</param>
        /// <returns>
        /// The code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </returns>
        IEnumerable<(string code, object value)> GetFieldInitializations(CompositionContext context);

        /// <summary>
        /// Returns the code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The whole composition context.</param>
        /// <returns>
        /// The code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </returns>
        IEnumerable<string> GetMethods(CompositionContext context);

        /// <summary>
        /// Returns the value indicating whether the <see cref="IComposition"/> requires a
        /// <see cref="ContructionContext{T}"/>.
        /// </summary>
        /// <param name="context">The whole composition context.</param>
        /// <returns>
        /// The value indicating whether the <see cref="IComposition"/> requires a <see cref="ContructionContext{T}"/>.
        /// </returns>
        bool RequiresContructionContext(CompositionContext context);
    }
}
