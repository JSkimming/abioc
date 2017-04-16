// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Registration extension methods on a <see cref="RegistrationContext{TContructionContext}"/>.
    /// </summary>
    public static class RegistrationContextExtensions
    {
        /// <summary>
        /// Registers an <paramref name="entry"/> for generation with the <paramref name="registration"/> context.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
        /// <param name="registration">The registration context.</param>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="entry"/>.<see cref="RegistrationEntry{TContructionContext}.ImplementationType"/>
        /// </param>
        /// <param name="entry">The entry to be registered.</param>
        /// <returns>The specified <paramref name="registration"/> to be used in a fluent configuration.</returns>
        public static RegistrationContext<TContructionContext> Register<TContructionContext>(
            this RegistrationContext<TContructionContext> registration,
            Type serviceType,
            RegistrationEntry<TContructionContext> entry)
            where TContructionContext : IContructionContext
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            List<RegistrationEntry<TContructionContext>> factories;
            if (!registration.Context.TryGetValue(serviceType, out factories))
            {
                factories = new List<RegistrationEntry<TContructionContext>>(1);
                registration.Context[serviceType] = factories;
            }

            factories.Add(entry);

            return registration;
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
        /// <param name="registration">The registration context.</param>
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
        /// <returns>The specified <paramref name="registration"/> to be used in a fluent configuration.</returns>
        public static RegistrationContext<TContructionContext> Register<TContructionContext>(
            this RegistrationContext<TContructionContext> registration,
            Type serviceType,
            Type implementationType,
            Func<TContructionContext, object> factory = null,
            bool typedfactory = false)
            where TContructionContext : IContructionContext
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return registration.Register(
                serviceType,
                RegistrationEntry.Create(implementationType, factory, typedfactory));
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
        /// <param name="registration">The registration context.</param>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedfactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        /// <returns>The specified <paramref name="registration"/> to be used in a fluent configuration.</returns>
        public static RegistrationContext<TContructionContext> Register<TContructionContext>(
            this RegistrationContext<TContructionContext> registration,
            Type implementationType,
            Func<TContructionContext, object> factory = null,
            bool typedfactory = false)
            where TContructionContext : IContructionContext
        {
            return registration.Register(implementationType, implementationType, factory, typedfactory);
        }
    }
}
