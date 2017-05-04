// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;

    /// <summary>
    /// Helper class used in the fluent composition of a registration.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    public class RegistrationComposerExtra<TExtra>
    {
        private bool _internal;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationComposerExtra{TExtra}"/> class.
        /// </summary>
        /// <param name="registration">The current <see cref="IRegistration"/>.</param>
        public RegistrationComposerExtra(IRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            Registration = registration;
        }

        /// <summary>
        /// Gets the current <see cref="IRegistration"/>.
        /// </summary>
        public IRegistration Registration { get; private set; }

        /// <summary>
        /// Sets the <see cref="Registration"/>.<see cref="IRegistration.Internal"/> property to <see langword="true"/>
        /// to indicate it is required as a dependency for other registrations but will not be resolved externally via
        /// a call to <see cref="IContainer.GetService"/> or <see cref="IContainer.GetServices"/>.
        /// </summary>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public RegistrationComposerExtra<TExtra> Internal()
        {
            Registration.Internal = _internal = true;
            return this;
        }

        /// <summary>
        /// Replaces the current <see cref="Registration"/> with the <paramref name="newRegistration"/>.
        /// </summary>
        /// <param name="newRegistration">The new <see cref="IRegistration"/>.</param>
        /// <returns>The old <see cref="IRegistration"/>.</returns>
        public IRegistration Replace(IRegistration newRegistration)
        {
            if (newRegistration == null)
                throw new ArgumentNullException(nameof(newRegistration));

            newRegistration.Internal = _internal;

            IRegistration oldRegistration = Registration;
            Registration = newRegistration;
            return oldRegistration;
        }
    }
}
