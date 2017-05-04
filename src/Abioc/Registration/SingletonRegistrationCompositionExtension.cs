// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods on <see cref="RegistrationComposer{T}"/> to use <see cref="SingletonRegistration"/>.
    /// </summary>
    public static class SingletonRegistrationCompositionExtension
    {
        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposer{T}.Registration"/> with a
        /// <see cref="SingletonRegistration"/>.
        /// </summary>
        /// <typeparam name="TImplementation">The <see cref="IRegistration.ImplementationType"/>.</typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <returns>The registration <paramref name="composer"/> to be used in a fluent configuration.</returns>
        public static RegistrationComposer<TImplementation> ToSingleton<TImplementation>(
            this RegistrationComposer<TImplementation> composer)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            composer.Replace(new SingletonRegistration(composer.Registration));
            return composer;
        }
    }
}
