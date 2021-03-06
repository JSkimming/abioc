﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The compiled context of function mappings.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    public class AbiocContainer<TExtra> : IContainer<TExtra>
    {
        /// <summary>
        /// The compiler generated GetService method.
        /// </summary>
#pragma warning disable SA1401 // Fields must be private
        public readonly IContainer<TExtra> GeneratedContainer;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Initializes a new instance of the <see cref="AbiocContainer{TExtra}"/> class.
        /// </summary>
        /// <param name="singleMappings">The compiled mapping from a type to a single create function.</param>
        /// <param name="multiMappings">
        /// The compiled mapping from a type to potentially multiple create functions.
        /// </param>
        /// <param name="generatedContainer">The runtime generated container.</param>
        public AbiocContainer(
            IReadOnlyDictionary<Type, Func<ConstructionContext<TExtra>, object>> singleMappings,
            IReadOnlyDictionary<Type, Func<ConstructionContext<TExtra>, object>[]> multiMappings,
            IContainer<TExtra> generatedContainer)
        {
            if (singleMappings == null)
                throw new ArgumentNullException(nameof(singleMappings));
            if (multiMappings == null)
                throw new ArgumentNullException(nameof(multiMappings));
            if (generatedContainer == null)
                throw new ArgumentNullException(nameof(generatedContainer));

            SingleMappings = singleMappings;
            MultiMappings = multiMappings;
            GeneratedContainer = generatedContainer;
        }

        /// <summary>
        /// Gets the compiled mapping from a type to a single create function.
        /// </summary>
        public IReadOnlyDictionary<Type, Func<ConstructionContext<TExtra>, object>> SingleMappings { get; }

        /// <summary>
        /// Gets the compiled mapping from a type to potentially multiple create functions.
        /// </summary>
        public IReadOnlyDictionary<Type, Func<ConstructionContext<TExtra>, object>[]> MultiMappings { get; }

        /// <summary>
        /// Gets any services that are defined in the <see cref="MultiMappings"/> for the
        /// <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <param name="extraData">The custom extra data used during construction.</param>
        /// <returns>
        /// Any services that are defined in the <see cref="MultiMappings"/> for the <paramref name="serviceType"/>.
        /// </returns>
        public IEnumerable<object> GetServices(Type serviceType, TExtra extraData)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return GeneratedContainer.GetServices(serviceType, extraData);
        }

        /// <summary>
        /// Gets any services that are defined in the <see cref="MultiMappings"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="extraData">The custom extra data used during construction.</param>
        /// <returns>
        /// Any services that are defined in the <see cref="MultiMappings"/> for the <typeparamref name="TService"/>.
        /// </returns>
        public IEnumerable<TService> GetServices<TService>(TExtra extraData)
        {
            return GetServices(typeof(TService), extraData).Cast<TService>();
        }

        /// <summary>
        /// Gets the service that is defined in the <see cref="SingleMappings"/> for the
        /// <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <param name="extraData">The custom extra data used during construction.</param>
        /// <returns>
        /// The service that is defined in the <see cref="SingleMappings"/> for the <paramref name="serviceType"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings for the <paramref name="serviceType"/>.</exception>
        public object GetService(Type serviceType, TExtra extraData)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            object service = GeneratedContainer.GetService(serviceType, extraData);
            if (service != null)
                return service;

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
        /// <param name="extraData">The custom extra data used during construction.</param>
        /// <returns>
        /// The service that is defined in the <see cref="SingleMappings"/> for the <typeparamref name="TService"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings for the <typeparamref name="TService"/>.</exception>
        public TService GetService<TService>(TExtra extraData)
        {
            return (TService)GetService(typeof(TService), extraData);
        }
    }
}
