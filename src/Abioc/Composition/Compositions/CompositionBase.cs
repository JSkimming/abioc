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
    public abstract class CompositionBase : IComposition
    {
        /// <inheritdoc />
        public abstract Type Type { get; }

        /// <inheritdoc />
        public abstract string GetInstanceExpression(GenerationContext context);

        /// <inheritdoc />
        public abstract string GetComposeMethodName(GenerationContext context);

        /// <inheritdoc />
        public virtual IEnumerable<string> GetMethods(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<(string snippet, object value)> GetFieldInitializations(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<(string, object)>();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAdditionalInitializations(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFields(GenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public abstract bool RequiresConstructionContext(GenerationContext context);
    }
}
