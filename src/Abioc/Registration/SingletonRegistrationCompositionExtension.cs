// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods on <see cref="RegistrationComposer"/> to use <see cref="SingletonRegistration"/>.
    /// </summary>
    public static class SingletonRegistrationCompositionExtension
    {
        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposer.Registration"/> with a
        /// <see cref="SingletonRegistration"/>.
        /// </summary>
        /// <param name="composer">The registration composer.</param>
        public static void ToSingleton(this RegistrationComposer composer)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            composer.Replace(new SingletonRegistration(composer.Registration));
        }

        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposerExtra{T}.Registration"/> with a
        /// <see cref="SingletonRegistration"/>.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="composer">The registration composer.</param>
        public static void ToSingleton<TExtra>(this RegistrationComposerExtra<TExtra> composer)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            composer.Replace(new SingletonRegistration(composer.Registration));
        }
    }
}
