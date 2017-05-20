// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition.Compositions;
    using Abioc.Registration;

    /// <summary>
    /// A composition registration visitor for a <see cref="FactoryRegistration"/>.
    /// </summary>
    public class FactoryRegistrationVisitor : IRegistrationVisitor<FactoryRegistration>
    {
        private CompositionContainer _container;

        /// <summary>
        /// Initializes the <see cref="IRegistrationVisitor"/>.
        /// </summary>
        /// <param name="container">The composition context.</param>
        public void Initialize(CompositionContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            _container = container;
        }

        /// <summary>
        /// Accepts the <see cref="FactoryRegistration"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="FactoryRegistration"/> to visit.</param>
        public void Accept(FactoryRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            Type type = registration.ImplementationType;
            var composition = new FactoryComposition(type, registration.Factory);
            _container.AddComposition(composition);
        }
    }
}
