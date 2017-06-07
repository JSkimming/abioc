// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;

    /// <summary>
    /// Factory generator for <see cref="IRegistrationVisitor"/>
    /// </summary>
    internal class VisitorManager
    {
        private static readonly MethodInfo VisitRegistrationMethodInfo =
            typeof(VisitorManager)
                .GetTypeInfo()
                .GetMethod(nameof(VisitRegistration), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, VisitRegistrationDelegate> VisitRegistrationDelegates =
            new ConcurrentDictionary<Type, VisitRegistrationDelegate>();

        private readonly CompositionContainer _container;

        private readonly VisitorFactory _factory;

        private readonly Dictionary<Type, IRegistrationVisitor[]> _visitors =
            new Dictionary<Type, IRegistrationVisitor[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VisitorManager"/> class.
        /// </summary>
        /// <param name="container">The <see cref="CompositionContainer"/>.</param>
        /// <param name="externalVisitorTypes">
        /// The types of concrete classes that implement <see cref="IRegistrationVisitor"/> from an external assembly.
        /// </param>
        public VisitorManager(CompositionContainer container, IEnumerable<Type> externalVisitorTypes = null)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            _container = container;
            _factory = new VisitorFactory(externalVisitorTypes);
        }

        private delegate void VisitRegistrationDelegate(VisitorManager manager, IRegistration registration);

        /// <summary>
        /// Gets the visitor for the <paramref name="registration"/> and processes it.
        /// </summary>
        /// <param name="registration">The <see cref="IRegistration"/> to visit.</param>
        public void Visit(IRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            VisitRegistrationDelegate visitorDelegate = GetVisitorDelegate(registration.GetType());
            visitorDelegate(this, registration);
        }

        private static VisitRegistrationDelegate GetVisitorDelegate(Type registrationType)
        {
            if (registrationType == null)
                throw new ArgumentNullException(nameof(registrationType));

            VisitRegistrationDelegate handler =
                VisitRegistrationDelegates.GetOrAdd(registrationType, CreateVisitorDelegate);
            return handler;
        }

        private static VisitRegistrationDelegate CreateVisitorDelegate(Type registrationType)
        {
            if (registrationType == null)
                throw new ArgumentNullException(nameof(registrationType));

            MethodInfo method = VisitRegistrationMethodInfo.MakeGenericMethod(registrationType);
            return (VisitRegistrationDelegate)method.CreateDelegate(typeof(VisitRegistrationDelegate));
        }

        private static void VisitRegistration<TRegistration>(VisitorManager manager, IRegistration registration)
            where TRegistration : class, IRegistration
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            Type visitorType = typeof(IRegistrationVisitor<TRegistration>);
            if (!manager._visitors.TryGetValue(visitorType, out IRegistrationVisitor[] visitors))
            {
                IEnumerable<IRegistrationVisitor> newVisitors =
                    manager._factory.CreateVisitors<TRegistration>(manager._container, manager);
                visitors = newVisitors.ToArray();

                if (visitors.Length == 0)
                {
                    string message = $"There are no visitors for registrations of type '{visitorType}'.";
                    throw new CompositionException(message);
                }

                manager._visitors[visitorType] = visitors;
            }

            foreach (var visitor in visitors.Cast<IRegistrationVisitor<TRegistration>>())
            {
                visitor.Accept((TRegistration)registration);
            }
        }
    }
}
