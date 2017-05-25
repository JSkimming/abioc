// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Composition.Compositions;
    using Abioc.Registration;

    /// <summary>
    /// A composition registration visitor for a <see cref="SingleConstructorRegistration"/>.
    /// </summary>
    internal class SingleConstructorRegistrationVisitor : IRegistrationVisitor<SingleConstructorRegistration>
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
        /// Accepts the <see cref="SingleConstructorRegistration"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="SingleConstructorRegistration"/> to visit.</param>
        public void Accept(SingleConstructorRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            Type type = registration.ImplementationType;

            // Since this is default registrations it can be superseded by other registrations. Therefore if there is
            // already a registration for this type, don't replace it.
            if (_container.Compositions.ContainsKey(type))
            {
                return;
            }

            TypeInfo typeInfo = type.GetTypeInfo();
            ConstructorInfo[] constructors = typeInfo.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // There must be a public constructor.
            if (constructors.Length == 0)
            {
                string message = $"The service of type '{type}' has no public constructors.";
                throw new CompositionException(message);
            }

            // There must be just 1 public constructor.
            if (constructors.Length > 1)
            {
                string message = $"The service of type '{type}' has {constructors.Length:N0} " +
                                 "public constructors. There must be just 1.";
                throw new CompositionException(message);
            }

            ParameterInfo[] parameters = constructors[0].GetParameters();
            IComposition composition = new ConstructorComposition(type, parameters, isDefault: true);
            _container.AddComposition(composition);
        }
    }
}
