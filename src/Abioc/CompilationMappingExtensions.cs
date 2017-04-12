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
    public static class CompilationMappingExtensions
    {
        /// <summary>
        /// Gets any services that are defined in the <paramref name="mapping"/> for the
        /// <paramref name="serviceType"/>.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the <paramref name="contructionContext"/>.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <param name="contructionContext">The construction context.</param>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>Any services for which there is a <paramref name="mapping"/>.</returns>
        public static IEnumerable<object> GetServices<TContructionContext>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<TContructionContext, object>>> mapping,
            TContructionContext contructionContext,
            Type serviceType)
            where TContructionContext : IContructionContext
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            // If there are any factories, use them.
            if (mapping.TryGetValue(serviceType, out IReadOnlyList<Func<TContructionContext, object>> factories))
            {
                return factories.Select(f => f(contructionContext));
            }

            // Otherwise return an empty enumerable to indicate there are no matches.
            return Enumerable.Empty<object>();
        }

        /// <summary>
        /// Gets the service that is defined in the <paramref name="mapping"/> for the <paramref name="serviceType"/>.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the <paramref name="contructionContext"/>.</typeparam>
        /// <param name="mapping">The compilation mapping.</param>
        /// <param name="contructionContext">The construction context.</param>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>
        /// The service that is defined in the <paramref name="mapping"/> for the <paramref name="serviceType"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings or more than one mapping.</exception>
        /// <remarks>
        /// There must be one and only one mapping defined for the <paramref name="serviceType"/>; otherwise a
        /// <see cref="DiException"/> is thrown.
        /// </remarks>
        public static object GetService<TContructionContext>(
            this IReadOnlyDictionary<Type, IReadOnlyList<Func<TContructionContext, object>>> mapping,
            TContructionContext contructionContext,
            Type serviceType)
            where TContructionContext : IContructionContext
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            string message;

            // If there are any factories, use them.
            if (mapping.TryGetValue(serviceType, out IReadOnlyList<Func<TContructionContext, object>> factories))
            {
                if (factories.Count == 1)
                {
                    return factories[0](contructionContext);
                }

                if (factories.Count > 1)
                {
                    message = $"There are multiple registered factories to create services of type '{serviceType}'.";
                    throw new DiException(message);
                }
            }

            message = $"There is no registered factory to create services of type '{serviceType}'.";
            throw new DiException(message);
        }
    }
}
