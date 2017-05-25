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
    /// A composition registration visitor for a <see cref="TypedFactoryRegistration{TExtra, TImplementation}"/>.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    /// <typeparam name="TImplementation">The <see cref="IRegistration.ImplementationType"/>.</typeparam>
    internal class TypedFactoryRegistrationVisitor<TExtra, TImplementation>
        : IRegistrationVisitor<TypedFactoryRegistration<TExtra, TImplementation>>
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
        /// Accepts the <see cref="TypedFactoryRegistration{TExtra, TImplementation}"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="TypedFactoryRegistration{TExtra, TImplementation}"/> to visit.</param>
        public void Accept(TypedFactoryRegistration<TExtra, TImplementation> registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            IComposition composition = new TypedFactoryComposition<TImplementation>(
                registration.Factory,
                typeof(ConstructionContext<TExtra>));
            _container.AddComposition(composition);
        }
    }
}
