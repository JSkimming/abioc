// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Generation;

    /// <summary>
    /// A base class that provides a default implementation of a <see cref="IComposition"/>.
    /// </summary>
    internal abstract class CompositionBase : IComposition
    {
        /// <inheritdoc />
        public abstract Type Type { get; }

        /// <inheritdoc />
        public abstract string GetInstanceExpression(IGenerationContext context);

        /// <inheritdoc />
        public abstract string GetComposeMethodName(IGenerationContext context);

        /// <inheritdoc />
        public virtual IEnumerable<string> GetMethods(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<(string snippet, object value)> GetFieldInitializations(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<(string, object)>();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAdditionalInitializations(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFields(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public abstract bool RequiresConstructionContext(IGenerationContext context);
    }
}
