// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper extension method for <see cref="IRegistration"/> instances.
    /// </summary>
    internal static class RegistrationExtensionHelpers
    {
        /// <summary>
        /// Returns the distinct types for all <paramref name="registrations"/>.
        /// </summary>
        /// <param name="registrations">The collection of <see cref="IRegistration"/> object.</param>
        /// <returns>The distinct types for all <paramref name="registrations"/>.</returns>
        public static IEnumerable<Type> DistinctRegistrationTypes(
            this IEnumerable<IRegistration> registrations)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            return registrations.Select(r => r.ImplementationType).Distinct();
        }

        /// <summary>
        /// Returns the distinct types for all public (where
        /// <see cref="IRegistration"/>.<see cref="IRegistration.Internal"/> is <see langword="false"/>)
        /// <paramref name="registrations"/>.
        /// </summary>
        /// <param name="registrations">The collection of <see cref="IRegistration"/> object.</param>
        /// <returns>The distinct types for all public <paramref name="registrations"/>.</returns>
        public static IEnumerable<Type> DistinctPublicRegistrationTypes(
            this IEnumerable<IRegistration> registrations)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            return registrations.Where(r => !r.Internal).Select(r => r.ImplementationType).Distinct();
        }
    }
}
