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
    /// A composition registration visitor for a <see cref="SingletonRegistration"/>.
    /// </summary>
    internal class SingletonRegistrationVisitor : IRegistrationVisitor<SingletonRegistration>
    {
        private readonly CompositionContainer _container;

        private readonly VisitorManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonRegistrationVisitor"/> class.
        /// </summary>
        /// <param name="container">The composition context.</param>
        /// <param name="manager">The visitor manager.</param>
        public SingletonRegistrationVisitor(CompositionContainer container, VisitorManager manager)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            _container = container;
            _manager = manager;
        }

        /// <summary>
        /// Accepts the <see cref="SingletonRegistration"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="SingletonRegistration"/> to visit.</param>
        public void Accept(SingletonRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            // Visit the inner registration which will add a composition.
            _manager.Visit(registration.Inner);

            // Get the original composition, removing it to allow it to be replaced.
            IComposition inner = _container.RemoveComposition(registration.ImplementationType);

            // Replace the inner composition.
            IComposition composition = new SingletonComposition(inner);
            _container.AddComposition(composition);
        }
    }
}
