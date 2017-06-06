// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Abioc.Composition.Compositions;
    using Abioc.Registration;

    /// <summary>
    /// A composition registration visitor for a <see cref="PropertyDependencyRegistration"/>.
    /// </summary>
    internal class PropertyDependencyRegistrationVisitor
        : IRegistrationVisitor<PropertyDependencyRegistration>
    {
        private readonly CompositionContainer _container;

        private readonly VisitorManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDependencyRegistrationVisitor"/> class.
        /// </summary>
        /// <param name="container">The composition context.</param>
        /// <param name="manager">The visitor manager.</param>
        public PropertyDependencyRegistrationVisitor(CompositionContainer container, VisitorManager manager)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            _container = container;
            _manager = manager;
        }

        /// <summary>
        /// Accepts the <see cref="PropertyDependencyRegistration"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="PropertyDependencyRegistration"/> to visit.</param>
        public void Accept(PropertyDependencyRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            // Visit the inner registration which will add a composition.
            _manager.Visit(registration.Inner);

            // Get the original composition, removing it to allow it to be replaced.
            IComposition inner = _container.RemoveComposition(registration.ImplementationType);

            (string property, Type type)[] propertiesToInject =
                GetPropertiesToInject(registration).Select(p => (p.Name, p.PropertyType)).ToArray();

            // Replace the inner composition.
            IComposition composition = new PropertyDependencyComposition(inner, propertiesToInject);
            _container.AddComposition(composition);
        }

        private static IEnumerable<PropertyInfo> GetPropertiesToInject(
            PropertyDependencyRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            if (registration.InjectAllProperties)
            {
                return registration.ImplementationType.GetTypeInfo().GetProperties().Where(p => p.CanWrite);
            }

            return
                from expression in registration.PropertyExpressions
                let memberExpression = (MemberExpression)expression.Body
                select (PropertyInfo)memberExpression.Member;
        }
    }
}
