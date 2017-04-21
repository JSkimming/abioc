// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods on <see cref="RegistrationComposer{T}"/> to use <see cref="TypedFactoryRegistration{T}"/>.
    /// </summary>
    public static class TypedFactoryRegistrationCompositionExtension
    {
        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposer{T}.Registration"/> with a
        /// <see cref="TypedFactoryRegistration{TImplementation}"/>.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// The type of the value provided by the <paramref name="factory"/>.
        /// </typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>.
        /// </param>
        /// <returns>The registration <paramref name="composer"/> to be used in a fluent configuration.</returns>
        public static RegistrationComposer<TImplementation> UseFactory<TImplementation>(
            this RegistrationComposer<TImplementation> composer,
            Func<TImplementation> factory)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            composer.Replace(new TypedFactoryRegistration<TImplementation>(factory));
            return composer;
        }

        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposer{T}.Registration"/> with a
        /// <see cref="TypedFactoryRegistration{TImplementation}"/>.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ContructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <typeparam name="TImplementation">
        /// The type of the value provided by the <paramref name="factory"/>.
        /// </typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>.
        /// </param>
        /// <returns>The registration <paramref name="composer"/> to be used in a fluent configuration.</returns>
        public static RegistrationComposer<TExtra, TImplementation> UseFactory<TExtra, TImplementation>(
            this RegistrationComposer<TExtra, TImplementation> composer,
            Func<ContructionContext<TExtra>, TImplementation> factory)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            composer.Replace(new TypedFactoryRegistration<TExtra, TImplementation>(factory));
            return composer;
        }
    }
}
