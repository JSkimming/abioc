// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The context to maintain the meta-data for an IoC context.
    /// </summary>
    /// <typeparam name="TContructionContext">The type of the context used during service resolution.</typeparam>
    public class RegistrationContext<TContructionContext>
        where TContructionContext : IContructionContext
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        public Dictionary<Type, List<RegistrationEntry<TContructionContext>>> Context { get; }
            = new Dictionary<Type, List<RegistrationEntry<TContructionContext>>>(32);

        /// <summary>
        /// Registers an <paramref name="entry"/> for generation with the registration context.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="entry"/>.<see cref="RegistrationEntry{TContructionContext}.ImplementationType"/>
        /// </param>
        /// <param name="entry">The entry to be registered.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationContext<TContructionContext> Register(
            Type serviceType,
            RegistrationEntry<TContructionContext> entry)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            List<RegistrationEntry<TContructionContext>> factories;
            if (!Context.TryGetValue(serviceType, out factories))
            {
                factories = new List<RegistrationEntry<TContructionContext>>(1);
                Context[serviceType] = factories;
            }

            factories.Add(entry);

            return this;
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="implementationType"/>.
        /// </param>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedfactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationContext<TContructionContext> Register(
            Type serviceType,
            Type implementationType,
            Func<TContructionContext, object> factory = null,
            bool typedfactory = false)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return Register(serviceType, RegistrationEntry.Create(implementationType, factory, typedfactory));
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedfactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationContext<TContructionContext> Register(
            Type implementationType,
            Func<TContructionContext, object> factory = null,
            bool typedfactory = false)
        {
            return Register(implementationType, implementationType, factory, typedfactory);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <typeparam name="TService">
        /// The type of the service to by satisfied during registration. The <typeparamref name="TService"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <typeparamref name="TImplementation"/>.
        /// </typeparam>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>. If not
        /// specified the an instance of <typeparamref name="TImplementation"/> will be automatically generated.
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationContext<TContructionContext> Register<TService, TImplementation>(
            Func<TContructionContext, TImplementation> factory = null)
            where TImplementation : class, TService
        {
            return Register(
                typeof(TService),
                typeof(TImplementation),
                factory,
                typedfactory: factory != null);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>. If not
        /// specified the an instance of <typeparamref name="TImplementation"/> will be automatically generated.
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationContext<TContructionContext> Register<TImplementation>(
            Func<TContructionContext, TImplementation> factory = null)
            where TImplementation : class
        {
            return Register(typeof(TImplementation), factory, typedfactory: factory != null);
        }
    }
}
