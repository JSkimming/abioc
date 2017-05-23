// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Generation;

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
        /// <param name="context">The context for code generation.</param>
        /// <returns>
        /// The code for the expression (e.g. method call or instance field) to retrieve an instance of the
        /// <see cref="Type"/>.
        /// </returns>
        string GetInstanceExpression(IGenerationContext context);

        /// <summary>
        /// Gets the name for the composition method, this is the method that will be called by an external service
        /// request.
        /// </summary>
        /// <param name="context">The context for code generation.</param>
        /// <returns>
        /// The name for the composition method, this is the method that will be called by an external service request.
        /// </returns>
        string GetComposeMethodName(IGenerationContext context);

        /// <summary>
        /// Returns the code of the methods required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The context for code generation.</param>
        /// <returns>
        /// The code of the methods required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </returns>
        IEnumerable<string> GetMethods(IGenerationContext context);

        /// <summary>
        /// Returns the code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The context for code generation.</param>
        /// <returns>
        /// The code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </returns>
        IEnumerable<string> GetFields(IGenerationContext context);

        /// <summary>
        /// Returns the code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </summary>
        /// <param name="context">The context for code generation.</param>
        /// <returns>
        /// The code of the fields required for the <see cref="IComposition"/> of the <see cref="Type"/>.
        /// </returns>
        IEnumerable<(string snippet, object value)> GetFieldInitializations(IGenerationContext context);

        /// <summary>
        /// Returns the code of additional initializations to the executes after the field initializations.
        /// </summary>
        /// <param name="context">The context for code generation.</param>
        /// <returns>The code of additional initializations to the executes after the field initializations.</returns>
        IEnumerable<string> GetAdditionalInitializations(IGenerationContext context);

        /// <summary>
        /// Returns the value indicating whether the <see cref="IComposition"/> requires a
        /// <see cref="ConstructionContext{T}"/>.
        /// </summary>
        /// <param name="context">The context for code generation.</param>
        /// <returns>
        /// The value indicating whether the <see cref="IComposition"/> requires a <see cref="ConstructionContext{T}"/>.
        /// </returns>
        bool RequiresConstructionContext(IGenerationContext context);
    }
}
