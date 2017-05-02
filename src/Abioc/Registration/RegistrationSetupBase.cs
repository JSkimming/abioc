// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Base class for a registration set-up.
    /// </summary>
    /// <typeparam name="TDerived">The type of the set-up derived from this class.</typeparam>
    public abstract class RegistrationSetupBase<TDerived>
        where TDerived : RegistrationSetupBase<TDerived>
    {
        /// <summary>
        /// Gets the setup registrations.
        /// </summary>
        public Dictionary<Type, List<IRegistration>> Registrations { get; }
            = new Dictionary<Type, List<IRegistration>>(32);

        /// <summary>
        /// Registers an <paramref name="entry"/> for generation with the <see cref="Registrations"/>.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="entry"/>.<see cref="IRegistration.ImplementationType"/>
        /// </param>
        /// <param name="entry">The entry to be registered.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived Register(Type serviceType, IRegistration entry)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            List<IRegistration> factories;
            if (!Registrations.TryGetValue(serviceType, out factories))
            {
                factories = new List<IRegistration>(1);
                Registrations[serviceType] = factories;
            }

            factories.Add(entry);

            return (TDerived)this;
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with the <see cref="Registrations"/>.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="implementationType"/>.
        /// </param>
        /// <param name="implementationType">The type of the implemented service to provide.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived Register(
            Type serviceType,
            Type implementationType,
            Action<RegistrationComposer<object>> compose = null)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            IRegistration defaultRegistration = new SingleConstructorRegistration(implementationType);

            if (compose == null)
            {
                return Register(serviceType, defaultRegistration);
            }

            var composer = new RegistrationComposer<object>(defaultRegistration);
            compose(composer);

            return Register(serviceType, composer.Registration);
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with the <see cref="Registrations"/>.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service to provide.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived Register(Type implementationType, Action<RegistrationComposer<object>> compose = null)
        {
            return Register(implementationType, implementationType, compose);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with the <see cref="Registrations"/>.
        /// </summary>
        /// <typeparam name="TService">
        /// The type of the service to by satisfied during registration. The <typeparamref name="TService"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <typeparamref name="TImplementation"/>.
        /// </typeparam>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived Register<TService, TImplementation>(
            Action<RegistrationComposer<TImplementation>> compose = null)
            where TImplementation : TService
        {
            IRegistration defaultRegistration = new SingleConstructorRegistration(typeof(TImplementation));

            if (compose == null)
            {
                return Register(typeof(TService), defaultRegistration);
            }

            var composer = new RegistrationComposer<TImplementation>(defaultRegistration);
            compose(composer);

            return Register(typeof(TService), composer.Registration);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with the <see cref="Registrations"/>.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived Register<TImplementation>(Action<RegistrationComposer<TImplementation>> compose = null)
            where TImplementation : class
        {
            return Register<TImplementation, TImplementation>(compose);
        }

        /// <summary>
        /// Registers an internal <paramref name="implementationType"/> for generation with the
        /// <see cref="Registrations"/>.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="implementationType"/>.
        /// </param>
        /// <param name="implementationType">The type of the implemented service to provide.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterInternal(
            Type serviceType,
            Type implementationType,
            Action<RegistrationComposer<object>> compose = null)
        {
            void InternalCompose(RegistrationComposer<object> composer)
            {
                compose?.Invoke(composer);
                composer.Internal();
            }

            return Register(serviceType, implementationType, InternalCompose);
        }

        /// <summary>
        /// Registers an internal <paramref name="implementationType"/> for generation with the
        /// <see cref="Registrations"/>.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service to provide.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterInternal(Type implementationType, Action<RegistrationComposer<object>> compose = null)
        {
            return RegisterInternal(implementationType, implementationType, compose);
        }

        /// <summary>
        /// Registers an internal <typeparamref name="TImplementation"/> for generation with the
        /// <see cref="Registrations"/>.
        /// </summary>
        /// <typeparam name="TService">
        /// The type of the service to by satisfied during registration. The <typeparamref name="TService"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <typeparamref name="TImplementation"/>.
        /// </typeparam>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterInternal<TService, TImplementation>(
            Action<RegistrationComposer<TImplementation>> compose = null)
            where TImplementation : TService
        {
            void InternalCompose(RegistrationComposer<TImplementation> composer)
            {
                compose?.Invoke(composer);
                composer.Internal();
            }

            return Register<TService, TImplementation>(InternalCompose);
        }

        /// <summary>
        /// Registers an internal <typeparamref name="TImplementation"/> for generation with the
        /// <see cref="Registrations"/>.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterInternal<TImplementation>(Action<RegistrationComposer<TImplementation>> compose = null)
            where TImplementation : class
        {
            return RegisterInternal<TImplementation, TImplementation>(compose);
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with a <paramref name="factory"/>
        /// provider.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="implementationType"/>.
        /// </param>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>.
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterFactory(Type serviceType, Type implementationType, Func<object> factory)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return Register(serviceType, implementationType, c => c.UseFactory(implementationType, factory));
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with a <paramref name="factory"/>
        /// provider.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>.
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterFactory(Type implementationType, Func<object> factory)
        {
            return RegisterFactory(implementationType, implementationType, factory);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with a <paramref name="factory"/>
        /// provider.
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
        public TDerived RegisterFactory<TService, TImplementation>(Func<TImplementation> factory)
            where TImplementation : class, TService
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return Register<TService, TImplementation>(c => c.UseFactory(factory));
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with a <paramref name="factory"/>
        /// provider.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>. If not
        /// specified the an instance of <typeparamref name="TImplementation"/> will be automatically generated.
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterFactory<TImplementation>(Func<TImplementation> factory)
            where TImplementation : class
        {
            return RegisterFactory<TImplementation, TImplementation>(factory);
        }

        /// <summary>
        /// Registers a fixed <paramref name="value"/> used as a singleton throughout constructions.
        /// </summary>
        /// <typeparam name="TService">
        /// The type of the service to by satisfied during registration. The <typeparamref name="TService"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <typeparamref name="TImplementation"/>.
        /// </typeparam>
        /// <typeparam name="TImplementation">The type of the fixed <paramref name="value"/>.</typeparam>
        /// <param name="value">
        /// The <see cref="InjectedSingletonRegistration{TImplementation}.Value"/> of type
        /// <typeparamref name="TImplementation"/>
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterFixed<TService, TImplementation>(TImplementation value)
            where TImplementation : TService
        {
            return Register<TService, TImplementation>(c => c.UseFixed(value));
        }

        /// <summary>
        /// Registers a fixed <paramref name="value"/> used as a singleton throughout constructions.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the fixed <paramref name="value"/>.</typeparam>
        /// <param name="value">
        /// The <see cref="InjectedSingletonRegistration{TImplementation}.Value"/> of type
        /// <typeparamref name="TImplementation"/>
        /// </param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived RegisterFixed<TImplementation>(TImplementation value)
        {
            return RegisterFixed<TImplementation, TImplementation>(value);
        }
    }
}
