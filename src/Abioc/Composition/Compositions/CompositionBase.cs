// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A base class that provides a default implementation of a <see cref="IComposition"/>.
    /// </summary>
    public abstract class CompositionBase : IComposition
    {
        /// <inheritdoc />
        public abstract Type Type { get; }

        /// <inheritdoc />
        public abstract string GetInstanceExpression(CompositionContainer container, bool simpleName);

        /// <inheritdoc />
        public abstract string GetComposeMethodName(CompositionContainer container, bool simpleName);

        /// <inheritdoc />
        public virtual IEnumerable<string> GetMethods(CompositionContainer container, bool simpleName)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<(string snippet, object value)> GetFieldInitializations(CompositionContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return Enumerable.Empty<(string, object)>();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAdditionalInitializations(CompositionContainer container, bool simpleName)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GetFields(CompositionContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public abstract bool RequiresConstructionContext(CompositionContainer container);
    }
}
