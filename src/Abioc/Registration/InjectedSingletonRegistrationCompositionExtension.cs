// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods on <see cref="RegistrationComposer{T}"/> to use
    /// <see cref="InjectedSingletonRegistration{T}"/>.
    /// </summary>
    public static class InjectedSingletonRegistrationCompositionExtension
    {
        /// <summary>
        /// Replaces the <paramref name="composer"/>.<see cref="RegistrationComposer{T}.Registration"/> with a
        /// <see cref="InjectedSingletonRegistration{TImplementation}"/>.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the <paramref name="value"/>.</typeparam>
        /// <param name="composer">The registration composer.</param>
        /// <param name="value">
        /// The <see cref="InjectedSingletonRegistration{TImplementation}.Value"/> of type
        /// <typeparamref name="TImplementation"/>
        /// </param>
        /// <returns>The registration <paramref name="composer"/> to be used in a fluent configuration.</returns>
        public static RegistrationComposer<TImplementation> UseFixed<TImplementation>(
            this RegistrationComposer<TImplementation> composer,
            TImplementation value)
        {
            if (composer == null)
                throw new ArgumentNullException(nameof(composer));

            composer.Replace(new InjectedSingletonRegistration<TImplementation>(value));
            return composer;
        }
    }
}
