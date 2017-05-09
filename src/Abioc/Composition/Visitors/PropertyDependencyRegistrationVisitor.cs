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
    public class PropertyDependencyRegistrationVisitor
        : IRegistrationVisitor<PropertyDependencyRegistration>, IRegistrationVisitorEx
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
        /// Accepts the <see cref="PropertyDependencyRegistration"/> to visit.
        /// </summary>
        /// <param name="registration">The <see cref="PropertyDependencyRegistration"/> to visit.</param>
        public void Accept(PropertyDependencyRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            (string property, Type type)[] propertiesToInject =
                GetPropertiesToInject(registration).Select(p => (p.Name, p.PropertyType)).ToArray();

            _manager.Visit(registration.Inner);

            // Get the original composition.
            IComposition inner = _context.Compositions[registration.ImplementationType];

            // Replace the inner composition.
            IComposition composition = new PropertyDependencyComposition(inner, propertiesToInject);
            _context.Compositions[composition.Type] = composition;
        }

        /// <inheritdoc />
        void IRegistrationVisitorEx.InitializeEx(VisitorManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            _manager = manager;
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
