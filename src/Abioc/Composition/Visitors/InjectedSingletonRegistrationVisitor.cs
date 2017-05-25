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
    /// A composition registration visitor for a <see cref="InjectedSingletonRegistration{TImplementation}"/>.
    /// </summary>
    /// <typeparam name="TImplementation">The <see cref="IRegistration.ImplementationType"/>.</typeparam>
    internal class InjectedSingletonRegistrationVisitor<TImplementation>
        : IRegistrationVisitor<InjectedSingletonRegistration<TImplementation>>
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
        /// Accepts the <see cref="InjectedSingletonRegistration{TImplementation}"/> to visit.
        /// </summary>
        /// <param name="registration">
        /// The <see cref="InjectedSingletonRegistration{TImplementation}"/> to visit.
        /// </param>
        public void Accept(InjectedSingletonRegistration<TImplementation> registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            IComposition composition = new InjectedSingletonComposition<TImplementation>(registration.Value);
            _container.AddComposition(composition);
        }
    }
}
