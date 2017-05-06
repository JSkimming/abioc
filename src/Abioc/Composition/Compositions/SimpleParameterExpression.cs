// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;

    /// <summary>
    /// The default parameter expression that defaults to the expression of a <see cref="IComposition"/>.
    /// </summary>
    internal class SimpleParameterExpression : IParameterExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterExpression"/> class.
        /// </summary>
        /// <param name="composition">The <see cref="Composition"/> used to satisfy the parameter expression.</param>
        public SimpleParameterExpression(IComposition composition)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            Composition = composition;
        }

        /// <summary>
        /// Gets the composition used to satisfy the parameter expression.
        /// </summary>
        public IComposition Composition { get; }

        /// <inheritdoc />
        public string GetInstanceExpression(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Composition.GetInstanceExpression(context, simpleName);
        }

        /// <inheritdoc />
        public bool RequiresConstructionContext(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Composition.RequiresConstructionContext(context);
        }
    }
}
