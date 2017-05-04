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
    public class SingletonRegistrationVisitor : IRegistrationVisitor<SingletonRegistration>, IRegistrationVisitorEx
    {
        private CompositionContext _context;

        private VisitorManager _manager;

        /// <summary>
        /// Initializes the <see cref="IRegistrationVisitor"/>.
        /// </summary>
        /// <param name="context">The composition context.</param>
        public void Initialize(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
        }

        /// <summary>
        /// Accepts the <see cref="SingletonRegistration"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="SingletonRegistration"/> to visit.</param>
        public void Accept(SingletonRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            _manager.Visit(registration.Inner);

            // Get the original composition.
            IComposition inner = _context.Compositions[registration.ImplementationType];

            // Replace the inner composition.
            IComposition composition = new SingletonComposition(inner);
            _context.Compositions[composition.Type] = composition;
        }

        /// <inheritdoc />
        void IRegistrationVisitorEx.InitializeEx(VisitorManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            _manager = manager;
        }
    }
}
