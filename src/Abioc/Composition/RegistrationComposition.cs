﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Registration;

    /// <summary>
    /// Composes a <see cref="RegistrationSetup"/> or <see cref="RegistrationSetup{T}"/> into a
    /// <see cref="CompositionContainer"/> for code generation.
    /// </summary>
    public static class RegistrationComposition
    {
        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContainer"/> for code
        /// generation.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static CompositionContainer Compose<TExtra>(this RegistrationSetup<TExtra> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations =
                setup.Registrations.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<IRegistration>)kvp.Value.ToArray());
            return registrations.Compose(typeof(TExtra), typeof(ConstructionContext<TExtra>));
        }

        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContainer"/> for code
        /// generation.
        /// </summary>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static CompositionContainer Compose(this RegistrationSetup setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations =
                setup.Registrations.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<IRegistration>)kvp.Value.ToArray());
            return registrations.Compose();
        }

        private static CompositionContainer Compose(
            this IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations,
            Type extraDataType = null,
            Type constructionContextType = null)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            var context = new CompositionContainer(registrations, extraDataType, constructionContextType);

            ProcessRegistrations(context);

            // Get any registrations that have not been composed, e.g. an interface mapped to a class.
            // Only use single mappings (Count == 1), multiple mappings cannot be composed.
            IEnumerable<(Type type, IRegistration registration)> typeMappings =
                from kvp in registrations
                where !context.Compositions.ContainsKey(kvp.Key) && kvp.Value.Count == 1
                select (kvp.Key, kvp.Value.Single());

            // Re-reference the compositions under the type mappings.
            foreach ((Type serviceType, IRegistration registration) in typeMappings)
            {
                context.AddComposition(serviceType, context.Compositions[registration.ImplementationType]);
            }

            return context;
        }

        private static void ProcessRegistrations(CompositionContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            var visitorManager = new VisitorManager(container);

            IEnumerable<IRegistration> registrations = container.Registrations.Values.SelectMany(r => r);
            foreach (IRegistration registration in registrations)
            {
                visitorManager.Visit(registration);
            }
        }
    }
}
