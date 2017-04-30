// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Collections;

#pragma warning disable SA1401 // Fields must be private

    /// <summary>
    /// The compiled context of function mappings.
    /// </summary>
    public class AbiocContainer
    {
        /// <summary>
        /// The compiled mapping from a type to a single create function.
        /// </summary>
        public readonly ImmutableHashTable<Type, Func<object>> SingleMappingsHash;

        /// <summary>
        /// The compiled mapping from a type to potentially multiple create functions.
        /// </summary>
        public readonly ImmutableHashTable<Type, Func<object>[]> MultiMappingsHash;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbiocContainer"/> class.
        /// </summary>
        /// <param name="singleMappings">The compiled mapping from a type to a single create function.</param>
        /// <param name="multiMappings">
        /// The compiled mapping from a type to potentially multiple create functions.
        /// </param>
        public AbiocContainer(
            IReadOnlyDictionary<Type, Func<object>> singleMappings,
            IReadOnlyDictionary<Type, Func<object>[]> multiMappings)
        {
            if (singleMappings == null)
                throw new ArgumentNullException(nameof(singleMappings));
            if (multiMappings == null)
                throw new ArgumentNullException(nameof(multiMappings));

            ////SingleMappings = singleMappings;
            ////MultiMappings = multiMappings;

            SingleMappingsHash = singleMappings.Aggregate(
                ImmutableHashTable<Type, Func<object>>.Empty,
                (current, mapping) => current.Add(mapping.Key, mapping.Value));

            MultiMappingsHash = multiMappings.Aggregate(
                ImmutableHashTable<Type, Func<object>[]>.Empty,
                (current, mapping) => current.Add(mapping.Key, mapping.Value));
        }

        /////// <summary>
        /////// Gets the compiled mapping from a type to a single create function.
        /////// </summary>
        ////public IReadOnlyDictionary<Type, Func<object>> SingleMappings { get; }

        /////// <summary>
        /////// Gets the compiled mapping from a type to potentially multiple create functions.
        /////// </summary>
        ////public IReadOnlyDictionary<Type, Func<object>[]> MultiMappings { get; }

        /// <summary>
        /// Gets any services that are defined for the <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>
        /// Any services that are defined for the <paramref name="serviceType"/>.
        /// </returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            Func<object>[] factories = MultiMappingsHash.Search(serviceType);

            // If there are any factories, use them; otherwise return an empty enumerable to indicate there are no
            // matches.
            return factories != null ? factories.Select(f => f()) : Enumerable.Empty<object>();
        }

        /// <summary>
        /// Gets any services that are defined for the <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <returns>
        /// Any services that are defined for the <typeparamref name="TService"/>.
        /// </returns>
        public IEnumerable<TService> GetServices<TService>()
        {
            return GetServices(typeof(TService)).Cast<TService>();
        }

        /// <summary>
        /// Gets the service that is defined for the <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>
        /// The service that is defined for the <paramref name="serviceType"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings for the <paramref name="serviceType"/>.</exception>
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            Func<object> factory = SingleMappingsHash.Search(serviceType);

            if (factory != null)
            {
                return factory();
            }

            // Produce a descriptive exception message, depending on where there are no mappings or multiple.
            if (MultiMappingsHash.Search(serviceType) == null)
            {
                throw new DiException($"There is no registered factory to create services of type '{serviceType}'.");
            }

            throw new DiException(
                $"There are multiple registered factories to create services of type '{serviceType}'.");
        }

        /// <summary>
        /// Gets the service that is defined for the <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <returns>
        /// The service that is defined for the <typeparamref name="TService"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings for the <typeparamref name="TService"/>.</exception>
        public TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }

        /// <summary>
        /// Gets the factory that is defined for the <paramref name="serviceType"/> or <see langword="null"/> if there
        /// are no factories for the <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the factory to get.</param>
        /// <returns>
        /// The factory that is defined for the <paramref name="serviceType"/>.
        /// </returns>
        public Func<object> GetFactory(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            Func<object> factory = SingleMappingsHash.Search(serviceType);
            return factory;
        }
    }
}
