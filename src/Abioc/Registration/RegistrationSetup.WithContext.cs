// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The registration setup for registrations that require a <see cref="ConstructionContext{TExtra}"/>.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    public class RegistrationSetup<TExtra> : RegistrationSetupBase<RegistrationSetup<TExtra>>
    {
        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with the
        /// <see cref="RegistrationSetupBase{TDerived}.Registrations"/>.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="implementationType"/>.
        /// </param>
        /// <param name="implementationType">The type of the implemented service to provide.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> Register(
            Type serviceType,
            Type implementationType,
            Action<RegistrationComposerExtra<TExtra>> compose = null)
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

            var composer = new RegistrationComposerExtra<TExtra>(defaultRegistration);
            compose(composer);

            return Register(serviceType, composer.Registration);
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with the
        /// <see cref="RegistrationSetupBase{TDerived}.Registrations"/>.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service to provide.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> Register(
            Type implementationType,
            Action<RegistrationComposerExtra<TExtra>> compose = null)
        {
            return Register(implementationType, implementationType, compose);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with the
        /// <see cref="RegistrationSetupBase{TDerived}.Registrations"/>.
        /// </summary>
        /// <typeparam name="TService">
        /// The type of the service to by satisfied during registration. The <typeparamref name="TService"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <typeparamref name="TImplementation"/>.
        /// </typeparam>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> Register<TService, TImplementation>(
            Action<RegistrationComposerExtra<TExtra, TImplementation>> compose = null)
            where TImplementation : TService
        {
            IRegistration defaultRegistration = new SingleConstructorRegistration(typeof(TImplementation));

            if (compose == null)
            {
                return Register(typeof(TService), defaultRegistration);
            }

            var composer = new RegistrationComposerExtra<TExtra, TImplementation>(defaultRegistration);
            compose(composer);

            return Register(typeof(TService), composer.Registration);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for generation with the
        /// <see cref="RegistrationSetupBase{TDerived}.Registrations"/>.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> Register<TImplementation>(
            Action<RegistrationComposerExtra<TExtra, TImplementation>> compose = null)
            where TImplementation : class
        {
            return Register<TImplementation, TImplementation>(compose);
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
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterFactory(
            Type serviceType,
            Type implementationType,
            Func<ConstructionContext<TExtra>, object> factory,
            Action<RegistrationComposerExtra<TExtra>> compose = null)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            void InternalCompose(RegistrationComposerExtra<TExtra> composer)
            {
                composer.UseFactory(implementationType, factory);
                compose?.Invoke(composer);
            }

            return Register(serviceType, implementationType, InternalCompose);
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for generation with a <paramref name="factory"/>
        /// provider.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>.
        /// </param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterFactory(
            Type implementationType,
            Func<ConstructionContext<TExtra>, object> factory,
            Action<RegistrationComposerExtra<TExtra>> compose = null)
        {
            return RegisterFactory(implementationType, implementationType, factory, compose);
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
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterFactory<TService, TImplementation>(
            Func<ConstructionContext<TExtra>, TImplementation> factory,
            Action<RegistrationComposerExtra<TExtra, TImplementation>> compose = null)
            where TImplementation : class, TService
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            void InternalCompose(RegistrationComposerExtra<TExtra, TImplementation> composer)
            {
                composer.UseFactory(factory);
                compose?.Invoke(composer);
            }

            return Register<TService, TImplementation>(InternalCompose);
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
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterFactory<TImplementation>(
            Func<ConstructionContext<TExtra>, TImplementation> factory,
            Action<RegistrationComposerExtra<TExtra, TImplementation>> compose = null)
            where TImplementation : class
        {
            return RegisterFactory<TImplementation, TImplementation>(factory, compose);
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for singleton generation. Only one value will ever be
        /// provided after the initial creation.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="implementationType"/>.
        /// </param>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterSingleton(
            Type serviceType,
            Type implementationType,
            Action<RegistrationComposerExtra<TExtra>> compose = null)
        {
            void InternalCompose(RegistrationComposerExtra<TExtra> composer)
            {
                compose?.Invoke(composer);
                composer.ToSingleton();
            }

            return Register(serviceType, implementationType, InternalCompose);
        }

        /// <summary>
        /// Registers an <paramref name="implementationType"/> for singleton generation. Only one value will ever be
        /// provided after the initial creation.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterSingleton(
            Type implementationType,
            Action<RegistrationComposerExtra<TExtra>> compose = null)
        {
            return RegisterSingleton(implementationType, implementationType, compose);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for singleton generation. Only one value will ever be
        /// provided after the initial creation.
        /// </summary>
        /// <typeparam name="TService">
        /// The type of the service to by satisfied during registration. The <typeparamref name="TService"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <typeparamref name="TImplementation"/>.
        /// </typeparam>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterSingleton<TService, TImplementation>(
            Action<RegistrationComposerExtra<TExtra, TImplementation>> compose = null)
            where TImplementation : class, TService
        {
            void InternalCompose(RegistrationComposerExtra<TExtra, TImplementation> composer)
            {
                compose?.Invoke(composer);
                composer.ToSingleton();
            }

            return Register<TService, TImplementation>(InternalCompose);
        }

        /// <summary>
        /// Registers an <typeparamref name="TImplementation"/> for singleton generation. Only one value will ever be
        /// provided after the initial creation.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implemented service.</typeparam>
        /// <param name="compose">The action to further compose the registration.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public RegistrationSetup<TExtra> RegisterSingleton<TImplementation>(
            Action<RegistrationComposerExtra<TExtra, TImplementation>> compose = null)
            where TImplementation : class
        {
            return RegisterSingleton<TImplementation, TImplementation>(compose);
        }
    }
}
