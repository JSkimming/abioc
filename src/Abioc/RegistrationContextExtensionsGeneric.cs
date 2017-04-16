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
    public static class RegistrationContextExtensionsGeneric
    {
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
        /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
        /// <param name="registration">The registration context.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>. If not
        /// specified the an instance of <typeparamref name="TImplementation"/> will be automatically generated.
        /// </param>
        /// <returns>The specified <paramref name="registration"/> to be used in a fluent configuration.</returns>
        public static RegistrationContext<TContructionContext> Register<TService, TImplementation, TContructionContext>(
            this RegistrationContext<TContructionContext> registration,
            Func<TContructionContext, TImplementation> factory = null)
            where TContructionContext : IContructionContext
            where TImplementation : class, TService
        {
            return registration.Register(typeof(TService), typeof(TImplementation), factory, typedfactory: true);
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
        /// <param name="registration">The registration context.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>. If not
        /// specified the an instance of <typeparamref name="TImplementation"/> will be automatically generated.
        /// </param>
        /// <returns>The specified <paramref name="registration"/> to be used in a fluent configuration.</returns>
        public static RegistrationContext<DefaultContructionContext> Register<TService, TImplementation>(
            this RegistrationContext<DefaultContructionContext> registration,
            Func<DefaultContructionContext, TImplementation> factory = null)
            where TImplementation : class, TService
        {
            return registration.Register<TService, TImplementation, DefaultContructionContext>(factory);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <typeparam name="TContructionContext">The type of the construction context.</typeparam>
        /// <param name="registration">The registration context.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>. If not
        /// specified the an instance of <typeparamref name="TImplementation"/> will be automatically generated.
        /// </param>
        /// <returns>The specified <paramref name="registration"/> to be used in a fluent configuration.</returns>
        public static RegistrationContext<TContructionContext> Register<TImplementation, TContructionContext>(
            this RegistrationContext<TContructionContext> registration,
            Func<TContructionContext, TImplementation> factory = null)
            where TContructionContext : IContructionContext
            where TImplementation : class
        {
            return registration.Register(typeof(TImplementation), factory, typedfactory: true);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with an optional
        /// <paramref name="factory"/> provider.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="registration">The registration context.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>. If not
        /// specified the an instance of <typeparamref name="TImplementation"/> will be automatically generated.
        /// </param>
        /// <returns>The specified <paramref name="registration"/> to be used in a fluent configuration.</returns>
        public static RegistrationContext<DefaultContructionContext> Register<TImplementation>(
            this RegistrationContext<DefaultContructionContext> registration,
            Func<DefaultContructionContext, TImplementation> factory = null)
            where TImplementation : class
        {
            return registration.Register<TImplementation, DefaultContructionContext>(factory);
        }
    }
}
