// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The compiled context of function mappings.
    /// </summary>
    /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
    public class CompilationContext<TContructionContext>
        where TContructionContext : IContructionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationContext{TContructionContext}"/> class.
        /// </summary>
        /// <param name="singleMappings">The compiled mapping from a type to a single create function.</param>
        /// <param name="multiMappings">
        /// The compiled mapping from a type to potentially multiple create functions.
        /// </param>
        public CompilationContext(
            IReadOnlyDictionary<Type, Func<TContructionContext, object>> singleMappings,
            IReadOnlyDictionary<Type, IReadOnlyList<Func<TContructionContext, object>>> multiMappings)
        {
            if (singleMappings == null)
                throw new ArgumentNullException(nameof(singleMappings));
            if (multiMappings == null)
                throw new ArgumentNullException(nameof(multiMappings));

            SingleMappings = singleMappings;
            MultiMappings = multiMappings;
        }

        /// <summary>
        /// Gets the compiled mapping from a type to a single create function.
        /// </summary>
        public IReadOnlyDictionary<Type, Func<TContructionContext, object>> SingleMappings { get; }

        /// <summary>
        /// Gets the compiled mapping from a type to potentially multiple create functions.
        /// </summary>
        public IReadOnlyDictionary<Type, IReadOnlyList<Func<TContructionContext, object>>> MultiMappings { get; }

        /// <summary>
        /// Gets any services that are defined in the <see cref="MultiMappings"/> for the
        /// <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="contructionContext">The construction context.</param>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>
        /// Any services that are defined in the <see cref="MultiMappings"/> for the <paramref name="serviceType"/>.
        /// </returns>
        public IEnumerable<object> GetServices(TContructionContext contructionContext, Type serviceType)
        {
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            // If there are any factories, use them.
            if (MultiMappings.TryGetValue(serviceType, out IReadOnlyList<Func<TContructionContext, object>> factories))
            {
                return factories.Select(f => f(contructionContext));
            }

            // Otherwise return an empty enumerable to indicate there are no matches.
            return Enumerable.Empty<object>();
        }

        /// <summary>
        /// Gets any services that are defined in the <see cref="MultiMappings"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="contructionContext">The construction context.</param>
        /// <returns>
        /// Any services that are defined in the <see cref="MultiMappings"/> for the <typeparamref name="TService"/>.
        /// </returns>
        public IEnumerable<TService> GetServices<TService>(
            TContructionContext contructionContext)
        {
            return GetServices(contructionContext, typeof(TService)).Cast<TService>();
        }

        /// <summary>
        /// Gets the service that is defined in the <see cref="SingleMappings"/> for the
        /// <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="contructionContext">The construction context.</param>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>
        /// The service that is defined in the <see cref="SingleMappings"/> for the <paramref name="serviceType"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings for the <paramref name="serviceType"/>.</exception>
        public object GetService(TContructionContext contructionContext, Type serviceType)
        {
            if (contructionContext == null)
                throw new ArgumentNullException(nameof(contructionContext));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (SingleMappings.TryGetValue(serviceType, out Func<TContructionContext, object> factory))
            {
                return factory(contructionContext);
            }

            // Produce a descriptive exception message, depending on where there are no mappings or multiple.
            if (!MultiMappings.ContainsKey(serviceType))
            {
                throw new DiException($"There is no registered factory to create services of type '{serviceType}'.");
            }

            throw new DiException(
                $"There are multiple registered factories to create services of type '{serviceType}'.");
        }

        /// <summary>
        /// Gets the service that is defined in the <see cref="SingleMappings"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="contructionContext">The construction context.</param>
        /// <returns>
        /// The service that is defined in the <see cref="SingleMappings"/> for the <typeparamref name="TService"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings for the <typeparamref name="TService"/>.</exception>
        public TService GetService<TService>(TContructionContext contructionContext)
        {
            return (TService)GetService(contructionContext, typeof(TService));
        }
    }
}
