// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    /// <summary>
    /// Helper class used in the fluent composition of a registration.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    /// <typeparam name="TImplementation">The <see cref="IRegistration.ImplementationType"/>.</typeparam>
    public class RegistrationComposerExtra<TExtra, TImplementation> : RegistrationComposerExtra<TExtra>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationComposerExtra{TExtra, TImplementation}"/> class.
        /// </summary>
        /// <param name="registration">The current <see cref="IRegistration"/>.</param>
        public RegistrationComposerExtra(IRegistration registration)
            : base(registration)
        {
        }

        /// <summary>
        /// Sets the <see cref="Registration"/>.<see cref="IRegistration.Internal"/> property to <see langword="true"/>
        /// to indicate it is required as a dependency for other registrations but will not be resolved externally via
        /// a call to <see cref="IContainer.GetService"/> or <see cref="IContainer.GetServices"/>.
        /// </summary>
        /// <returns>This registration composer to be used in a fluent configuration.</returns>
        public new RegistrationComposerExtra<TExtra, TImplementation> Internal()
        {
            base.Internal();
            return this;
        }
    }
}
