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
    public static class CompilationMappingExtensionsGeneric
    {
        /// <summary>
        /// Gets any services that are defined in the <paramref name="mapping"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the <paramref name="contructionContext"/>.</typeparam>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <param name="contructionContext">The construction context.</param>
        /// <returns>Any services for which there is a <paramref name="mapping"/>.</returns>
        public static IEnumerable<TService> GetServices<TContructionContext, TService>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<TContructionContext, object>>> mapping,
            TContructionContext contructionContext)
            where TContructionContext : IContructionContext
        {
            return mapping.GetServices(contructionContext, typeof(TService)).Cast<TService>();
        }

        /// <summary>
        /// Gets any services that are defined in the <paramref name="mapping"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <param name="contructionContext">The construction context.</param>
        /// <returns>Any services for which there is a <paramref name="mapping"/>.</returns>
        public static IEnumerable<TService> GetServices<TService>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mapping,
            DefaultContructionContext contructionContext)
        {
            return mapping.GetServices<DefaultContructionContext, TService>(contructionContext);
        }

        /// <summary>
        /// Gets any services that are defined in the <paramref name="mapping"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <returns>Any services for which there is a <paramref name="mapping"/>.</returns>
        public static IEnumerable<TService> GetServices<TService>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mapping)
        {
            return mapping.GetServices<TService>(new DefaultContructionContext());
        }

        /// <summary>
        /// Gets the service that is defined in the <paramref name="mapping"/> for the <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the <paramref name="contructionContext"/>.</typeparam>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <param name="contructionContext">The construction context.</param>
        /// <returns>
        /// The service that is defined in the <paramref name="mapping"/> for the <typeparamref name="TService"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings or more than one mapping.</exception>
        /// <remarks>
        /// There must be one and only one mapping defined for the <typeparamref name="TService"/>; otherwise a
        /// <see cref="DiException"/> is thrown.
        /// </remarks>
        public static TService GetService<TContructionContext, TService>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<TContructionContext, object>>> mapping,
            TContructionContext contructionContext)
            where TContructionContext : IContructionContext
        {
            return (TService)mapping.GetService(contructionContext, typeof(TService));
        }

        /// <summary>
        /// Gets the service that is defined in the <paramref name="mapping"/> for the <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <param name="contructionContext">The construction context.</param>
        /// <returns>
        /// The service that is defined in the <paramref name="mapping"/> for the <typeparamref name="TService"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings or more than one mapping.</exception>
        /// <remarks>
        /// There must be one and only one mapping defined for the <typeparamref name="TService"/>; otherwise a
        /// <see cref="DiException"/> is thrown.
        /// </remarks>
        public static TService GetService<TService>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mapping,
            DefaultContructionContext contructionContext)
        {
            return mapping.GetService<DefaultContructionContext, TService>(contructionContext);
        }

        /// <summary>
        /// Gets the service that is defined in the <paramref name="mapping"/> for the <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <returns>
        /// The service that is defined in the <paramref name="mapping"/> for the <typeparamref name="TService"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings or more than one mapping.</exception>
        /// <remarks>
        /// There must be one and only one mapping defined for the <typeparamref name="TService"/>; otherwise a
        /// <see cref="DiException"/> is thrown.
        /// </remarks>
        public static TService GetService<TService>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mapping)
        {
            return mapping.GetService<TService>(new DefaultContructionContext());
        }
    }
}
