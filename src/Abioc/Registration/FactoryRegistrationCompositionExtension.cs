// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods on <see cref="RegistrationComposer"/> to use <see cref="FactoryRegistration"/>.
    /// </summary>
    public static class FactoryRegistrationCompositionExtension
    {
        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposer.Registration"/> with a
        /// <see cref="FactoryRegistration"/>.
        /// </summary>
        /// <param name="composer">The registration composer.</param>
        /// <param name="implementationType">The type of the value provided by the <paramref name="factory"/>.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>.
        /// </param>
        /// <returns>The registration <paramref name="composer"/> to be used in a fluent configuration.</returns>
        public static RegistrationComposer UseFactory(
            this RegistrationComposer composer,
            Type implementationType,
            Func<object> factory)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            composer.Replace(new FactoryRegistration(implementationType, factory));
            return composer;
        }

        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposerExtra{T}.Registration"/> with a
        /// <see cref="FactoryRegistration"/>.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <param name="implementationType">The type of the value provided by the <paramref name="factory"/>.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>.
        /// </param>
        /// <returns>The registration <paramref name="composer"/> to be used in a fluent configuration.</returns>
        public static RegistrationComposerExtra<TExtra> UseFactory<TExtra>(
            this RegistrationComposerExtra<TExtra> composer,
            Type implementationType,
            Func<ConstructionContext<TExtra>, object> factory)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            composer.Replace(new FactoryRegistration<TExtra>(implementationType, factory));
            return composer;
        }
    }
}
