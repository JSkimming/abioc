// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Resolve extension methods on a compilation mapping.
    /// </summary>
    internal static class CompilationMappingExtensions
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CompilationContext{TContructionContext}"/> class.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
        /// <param name="multiMappings">
        /// The compiled mapping from a type to potentially multiple create functions.
        /// </param>
        /// <returns>A new instance of the <see cref="CompilationContext{TContructionContext}"/> class.</returns>
        public static CompilationContext<TContructionContext> ToCompilationContext<TContructionContext>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<TContructionContext, object>>> multiMappings)
            where TContructionContext : IContructionContext
        {
            if (multiMappings == null)
                throw new ArgumentNullException(nameof(multiMappings));

            Dictionary<Type, Func<TContructionContext, object>> singleMappings =
                multiMappings
                    .Where(kvp => kvp.Value.Count == 1)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Single());

            return new CompilationContext<TContructionContext>(singleMappings, multiMappings);
        }
    }
}
