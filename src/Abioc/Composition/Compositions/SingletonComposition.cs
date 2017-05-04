// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A composition to produce code for a singleton value.
    /// </summary>
    public class SingletonComposition : IComposition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonComposition"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="Inner"/> <see cref="IComposition"/>.</param>
        public SingletonComposition(IComposition inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            Inner = inner;
        }

        /// <summary>
        /// Gets the <see cref="Inner"/> <see cref="IComposition"/>.
        /// </summary>
        public IComposition Inner { get; }

        /// <summary>
        /// Gets the type provided by the <see cref="Inner"/> <see cref="IComposition"/>.
        /// </summary>
        public Type Type => Inner.Type;

        /// <inheritdoc />
        public string GetInstanceExpression(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetInstanceExpression(context, simpleName);
        }

        /// <inheritdoc />
        public string GetComposeMethodName(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetComposeMethodName(context, simpleName);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetMethods(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetMethods(context, simpleName);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetFields(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetFields(context);
        }

        /// <inheritdoc />
        public IEnumerable<(string snippet, object value)> GetFieldInitializations(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetFieldInitializations(context);
        }

        /// <inheritdoc />
        public bool RequiresConstructionContext(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.RequiresConstructionContext(context);
        }
    }
}
