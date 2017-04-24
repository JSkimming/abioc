// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;

    /// <summary>
    /// A composition registration visitor for a <see cref="SingleConstructorRegistration"/>.
    /// </summary>
    public class SingleConstructorRegistrationVisitor : IRegistrationVisitor<SingleConstructorRegistration>
    {
        private CompositionContext _context;

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
        /// Accepts the <see cref="SingleConstructorRegistration"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="SingleConstructorRegistration"/> to visit.</param>
        public void Accept(SingleConstructorRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            Type type = registration.ImplementationType;
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

            ConstructorInfo constructorInfo = constructors[0];
            ParameterInfo[] parameters = constructorInfo.GetParameters();

            _context.Compositions[type] = new ConstructorComposition(type, constructorInfo, parameters);
        }
    }
}
