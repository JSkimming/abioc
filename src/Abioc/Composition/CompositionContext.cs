// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The composition context.
    /// </summary>
    public class CompositionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionContext"/> class.
        /// </summary>
        /// <param name="contructionContext">The type of the <see cref="ContructionContext{T}"/>.</param>
        public CompositionContext(string contructionContext = null)
        {
            ContructionContext = contructionContext ?? string.Empty;
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        public Dictionary<Type, IComposition> Compositions { get; } = new Dictionary<Type, IComposition>(32);

        /// <summary>
        /// Gets the type of the <see cref="ContructionContext{T}"/>.
        /// </summary>
        public string ContructionContext { get; }
    }
}
