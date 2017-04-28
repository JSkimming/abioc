// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods on <see cref="AbiocContainer"/> and <see cref="AbiocContainer{T}"/>.
    /// </summary>
    internal static class AbiocContainerExtensions
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AbiocContainer"/> class.
        /// </summary>
        /// <param name="multiMappings">
        /// The compiled mapping from a type to potentially multiple create functions.
        /// </param>
        /// <returns>A new instance of the <see cref="AbiocContainer"/> class.</returns>
        public static AbiocContainer ToContainer(
            this IReadOnlyDictionary<Type, Func<object>[]> multiMappings)
        {
            if (multiMappings == null)
                throw new ArgumentNullException(nameof(multiMappings));

            Dictionary<Type, Func<object>> singleMappings =
                multiMappings
                    .Where(kvp => kvp.Value.Length == 1)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Single());

            return new AbiocContainer(singleMappings, multiMappings);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AbiocContainer{T}"/> class.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="multiMappings">
        /// The compiled mapping from a type to potentially multiple create functions.
        /// </param>
        /// <returns>A new instance of the <see cref="AbiocContainer"/> class.</returns>
        public static AbiocContainer<TExtra> ToContainer<TExtra>(
            this IReadOnlyDictionary<Type, Func<ConstructionContext<TExtra>, object>[]> multiMappings)
        {
            if (multiMappings == null)
                throw new ArgumentNullException(nameof(multiMappings));

            Dictionary<Type, Func<ConstructionContext<TExtra>, object>> singleMappings =
                multiMappings
                    .Where(kvp => kvp.Value.Length == 1)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Single());

            return new AbiocContainer<TExtra>(singleMappings, multiMappings);
        }
    }
}
