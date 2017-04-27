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
    /// Composes a <see cref="RegistrationSetup"/> or <see cref="RegistrationSetup{T}"/> into a
    /// <see cref="CompositionContext"/> for code generation.
    /// </summary>
    public static class RegistrationComposition
    {
        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContext"/> for code
        /// generation.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <returns>The <see cref="CompositionContext"/>.</returns>
        public static CompositionContext Compose<TExtra>(this RegistrationSetup<TExtra> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            return setup.Registrations.Compose(typeof(ConstructionContext<TExtra>).ToCompileName());
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
            string constructionContext = null)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            var context = new CompositionContext(constructionContext);

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

            var visitorManager = new VisitorManager(context);

            IEnumerable<IRegistration> distinctRegistrations =
                registrations.Values.SelectMany(r => r).DistinctBy(r => r.ImplementationType);
            foreach (IRegistration registration in distinctRegistrations)
            {
                visitorManager.Visit(registration);
            }
        }

        private static Dictionary<Type, List<IRegistrationVisitor>> GetVisitors(
            CompositionContext context)
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
                TypeInfo typeInfo = visitorType.GetTypeInfo();

                IEnumerable<Type> interfaceTypes =
                    typeInfo
                        .GetInterfaces()
                        .Where(i => i.GetTypeInfo().IsGenericType
                                    && typeof(IRegistrationVisitor<>) == i.GetGenericTypeDefinition());

                if (typeInfo.ContainsGenericParameters)
                {
                    Type[] parameters = typeInfo.GenericTypeParameters;
                    Type[] arguments = typeInfo.GenericTypeArguments;
                }

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

        private static IRegistrationVisitor CreateVisitor(Type visitorType)
        {
            if (visitorType == null)
                throw new ArgumentNullException(nameof(visitorType));

            var visitor = (IRegistrationVisitor)Activator.CreateInstance(visitorType);
            return visitor;
        }

        private static IRegistrationVisitor CreateVisitorWithContext(Type visitorType, Type extraType)
        {
            if (visitorType == null)
                throw new ArgumentNullException(nameof(visitorType));
            if (extraType == null)
                throw new ArgumentNullException(nameof(extraType));

            TypeInfo typeInfo = visitorType.GetTypeInfo();
            Type genericType = typeInfo.MakeGenericType(extraType);
            return CreateVisitor(genericType);
        }

        private static IRegistrationVisitor CreateTypedVisitor(Type visitorType, Type implementationType)
        {
            if (visitorType == null)
                throw new ArgumentNullException(nameof(visitorType));
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            TypeInfo typeInfo = visitorType.GetTypeInfo();
            Type genericType = typeInfo.MakeGenericType(implementationType);
            return CreateVisitor(genericType);
        }

        private static IRegistrationVisitor CreateTypedVisitorWithContext(
            Type visitorType,
            Type extraType,
            Type implementationType)
        {
            if (visitorType == null)
                throw new ArgumentNullException(nameof(visitorType));
            if (extraType == null)
                throw new ArgumentNullException(nameof(extraType));
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            TypeInfo typeInfo = visitorType.GetTypeInfo();
            Type genericType = typeInfo.MakeGenericType(extraType, implementationType);
            return CreateVisitor(genericType);
        }
    }
}
