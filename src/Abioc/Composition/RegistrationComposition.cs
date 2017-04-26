﻿// Copyright (c) 2017 James Skimming. All rights reserved.
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
    /// Composes a <see cref="RegistrationSetup"/> or <see cref="RegistrationSetup{T}"/> into a
    /// <see cref="CompositionContext"/> for code generation.
    /// </summary>
    public static class RegistrationComposition
    {
        private static readonly MethodInfo VisitRegistrationMethodInfo =
            typeof(RegistrationComposition).GetTypeInfo().GetMethod(
                nameof(VisitRegistration),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, VisitRegistrationDelegate> VisitRegistrationDelegates =
            new ConcurrentDictionary<Type, VisitRegistrationDelegate>();

        private delegate void VisitRegistrationDelegate(
            IRegistration registration,
            Dictionary<Type, List<IRegistrationVisitor>> visitors);

        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContext"/> for code
        /// generation.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ContructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <returns>The <see cref="CompositionContext"/>.</returns>
        public static CompositionContext Compose<TExtra>(this RegistrationSetup<TExtra> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            return setup.Registrations.Compose(typeof(ContructionContext<TExtra>).ToCompileName());
        }

        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContext"/> for code
        /// generation.
        /// </summary>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <returns>The <see cref="CompositionContext"/>.</returns>
        public static CompositionContext Compose(this RegistrationSetup setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            return setup.Registrations.Compose();
        }

        private static CompositionContext Compose(
            this IReadOnlyDictionary<Type, List<IRegistration>> registrations,
            string contructionContext = null)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            var context = new CompositionContext(contructionContext);

            ProcessRegistrations(registrations, context);

            // Get any registrations that have not been composed, e.g. an interface mapped to a class.
            // Only use single mappings (Count == 1), multiple mappings cannot be composed.
            IEnumerable<(Type type, IRegistration registration)> typeMappings =
                from kvp in registrations
                where !context.Compositions.ContainsKey(kvp.Key) && kvp.Value.Count == 1
                select (kvp.Key, kvp.Value.Single());

            // Re-reference the compositions under the type mappings.
            foreach ((Type type, IRegistration registration) in typeMappings)
            {
                context.Compositions[type] = context.Compositions[registration.ImplementationType];
            }

            return context;
        }

        private static void ProcessRegistrations(
            IReadOnlyDictionary<Type, List<IRegistration>> registrations,
            CompositionContext context)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Dictionary<Type, List<IRegistrationVisitor>> visitors = GetVisitors(context);

            IEnumerable<IRegistration> distinctRegistrations =
                registrations.Values.SelectMany(r => r).DistinctBy(r => r.ImplementationType);
            foreach (IRegistration registration in distinctRegistrations)
            {
                VisitRegistrationDelegate visitorDelegate = GetVisitorDelegate(registration.GetType());
                visitorDelegate(registration, visitors);
            }
        }

        private static Dictionary<Type, List<IRegistrationVisitor>> GetVisitors(CompositionContext context)
        {
            Assembly assembly = typeof(IRegistrationVisitor).GetTypeInfo().Assembly;
            IEnumerable<Type> visitorTypes =
                from t in assembly.GetTypes().Where(typeof(IRegistrationVisitor).GetTypeInfo().IsAssignableFrom)
                let ti = t.GetTypeInfo()
                where ti.IsClass && !ti.IsAbstract
                select t;

            var visitors = new Dictionary<Type, List<IRegistrationVisitor>>();

            foreach (var visitorType in visitorTypes)
            {
                IEnumerable<Type> interfaceTypes =
                    visitorType.GetTypeInfo()
                        .GetInterfaces()
                        .Where(i => i.GetTypeInfo().IsGenericType
                                    && typeof(IRegistrationVisitor<>) == i.GetGenericTypeDefinition());

                var visitor = (IRegistrationVisitor)Activator.CreateInstance(visitorType);
                visitor.Initialize(context);

                foreach (Type interfaceType in interfaceTypes)
                {
                    List<IRegistrationVisitor> list;
                    if (!visitors.TryGetValue(interfaceType, out list))
                    {
                        list = new List<IRegistrationVisitor>(1);
                        visitors[interfaceType] = list;
                    }

                    list.Add(visitor);
                }
            }

            return visitors;
        }

        private static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> items,
            Func<TSource, TKey> keySelector)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return items.GroupBy(keySelector).Select(item => item.First());
        }

        private static VisitRegistrationDelegate GetVisitorDelegate(Type jobType)
        {
            if (jobType == null)
                throw new ArgumentNullException(nameof(jobType));

            VisitRegistrationDelegate handler = VisitRegistrationDelegates.GetOrAdd(jobType, CreateVisitorDelegate);
            return handler;
        }

        private static VisitRegistrationDelegate CreateVisitorDelegate(Type registrationType)
        {
            if (registrationType == null)
                throw new ArgumentNullException(nameof(registrationType));

            MethodInfo method = VisitRegistrationMethodInfo.MakeGenericMethod(registrationType);
            return (VisitRegistrationDelegate)method.CreateDelegate(typeof(VisitRegistrationDelegate));
        }

        private static void VisitRegistration<TRegistration>(
            IRegistration registration,
            Dictionary<Type, List<IRegistrationVisitor>> visitors)
            where TRegistration : class, IRegistration
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            if (visitors == null)
                throw new ArgumentNullException(nameof(visitors));

            Type registrationType = typeof(IRegistrationVisitor<TRegistration>);
            if (!visitors.TryGetValue(registrationType, out var list))
            {
                string message = $"There are no visitors for registrations of type '{registrationType}'.";
                throw new CompositionException(message);
            }

            foreach (var visitor in list.Cast<IRegistrationVisitor<TRegistration>>())
            {
                visitor.Accept((TRegistration)registration);
            }
        }
    }
}