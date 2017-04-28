// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides a set of <see langword="static"/> extension methods for querying objects that implement
    /// <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the default equality comparer for the projected type.
        /// </summary>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="items">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> items,
            Func<TSource, TKey> keySelector)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return items.GroupBy(keySelector).Select(item => item.First());
        }
    }
}
