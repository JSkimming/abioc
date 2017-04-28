// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper class used in the fluent composition of a registration.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    /// <typeparam name="TImplementation">The <see cref="IRegistration.ImplementationType"/>.</typeparam>
    public class RegistrationComposer<TExtra, TImplementation> : RegistrationComposer<TImplementation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationComposer{TExtra, TImplementation}"/> class.
        /// </summary>
        /// <param name="registration">The current <see cref="IRegistration"/>.</param>
        public RegistrationComposer(IRegistration registration)
            : base(registration)
        {
        }
    }
}
