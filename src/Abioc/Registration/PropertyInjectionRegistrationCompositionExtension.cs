// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Extension methods on <see cref="RegistrationComposer"/> to use property injection.
    /// </summary>
    public static class PropertyInjectionRegistrationCompositionExtension
    {
        /// <summary>
        /// Specifies that a <typeparamref name="TProp"/> <paramref name="property"/> of an
        /// <typeparamref name="TImplementation"/> needs to be injected as a dependency.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// The type of the implementation to receive the injected <paramref name="property"/>.
        /// </typeparam>
        /// <typeparam name="TProp">The type of the property to be injected.</typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <param name="property">The expression used to specify the property to inject.</param>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public static RegistrationComposer<TImplementation> InjectProperty<TImplementation, TProp>(
            this RegistrationComposer<TImplementation> composer,
            Expression<Func<TImplementation, TProp>> property)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (composer.Registration is PropertyDependencyRegistration registration)
            {
                registration.AddInjectedProperty(property);
            }
            else
            {
                composer.Replace(new PropertyDependencyRegistration(composer.Registration, property));
            }

            return composer;
        }

        /// <summary>
        /// Specifies that all the properties of a service need to be injected as a dependency.
        /// </summary>
        /// <param name="composer">The registration composer.</param>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public static RegistrationComposer InjectAllProperties(this RegistrationComposer composer)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            if (composer.Registration is PropertyDependencyRegistration)
            {
                string message =
                    $"Cannot inject all properties of '{composer.Registration.ImplementationType}' if existing " +
                    "property injection dependencies have already been registered.";
                throw new RegistrationException(message);
            }

            composer.Replace(new PropertyDependencyRegistration(composer.Registration));
            return composer;
        }

        /// <summary>
        /// Specifies that all the properties of an <typeparamref name="TImplementation"/> need to be injected as a
        /// dependency.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// The type of the implementation to receive the injected properties.
        /// </typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public static RegistrationComposer<TImplementation> InjectAllProperties<TImplementation>(
            this RegistrationComposer<TImplementation> composer)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            ((RegistrationComposer)composer).InjectAllProperties();
            return composer;
        }

        /// <summary>
        /// Specifies that a <typeparamref name="TProp"/> <paramref name="property"/> of an
        /// <typeparamref name="TImplementation"/> needs to be injected as a dependency.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <typeparam name="TImplementation">
        /// The type of the implementation to receive the injected <paramref name="property"/>.
        /// </typeparam>
        /// <typeparam name="TProp">The type of the property to be injected.</typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <param name="property">The expression used to specify the property to inject.</param>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public static RegistrationComposerExtra<TExtra, TImplementation> InjectProperty<TExtra, TImplementation, TProp>(
            this RegistrationComposerExtra<TExtra, TImplementation> composer,
            Expression<Func<TImplementation, TProp>> property)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (composer.Registration is PropertyDependencyRegistration registration)
            {
                registration.AddInjectedProperty(property);
            }
            else
            {
                composer.Replace(new PropertyDependencyRegistration(composer.Registration, property));
            }

            return composer;
        }

        /// <summary>
        /// Specifies that all the properties of a service need to be injected as a dependency.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public static RegistrationComposerExtra<TExtra> InjectAllProperties<TExtra>(
            this RegistrationComposerExtra<TExtra> composer)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            if (composer.Registration is PropertyDependencyRegistration)
            {
                string message =
                    $"Cannot inject all properties of '{composer.Registration.ImplementationType}' if existing " +
                    "property injection dependencies have already been registered.";
                throw new RegistrationException(message);
            }

            composer.Replace(new PropertyDependencyRegistration(composer.Registration));
            return composer;
        }

        /// <summary>
        /// Specifies that all the properties of an <typeparamref name="TImplementation"/> need to be injected as a
        /// dependency.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <typeparam name="TImplementation">
        /// The type of the implementation to receive the injected properties.
        /// </typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public static RegistrationComposerExtra<TExtra, TImplementation> InjectAllProperties<TExtra, TImplementation>(
            this RegistrationComposerExtra<TExtra, TImplementation> composer)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            ((RegistrationComposerExtra<TExtra>)composer).InjectAllProperties();
            return composer;
        }
    }
}
