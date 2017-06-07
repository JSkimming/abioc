// Copyright (c) 2017 James Skimming. All rights reserved.
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
        /// <param name="externalVisitorTypes">
        /// The types of concrete classes that implement <see cref="IRegistrationVisitor"/> from an external assembly.
        /// </param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static CompositionContainer Compose<TExtra>(
            this RegistrationSetup<TExtra> setup,
            params Type[] externalVisitorTypes)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (externalVisitorTypes == null)
                throw new ArgumentNullException(nameof(externalVisitorTypes));

            return Compose(setup, (IEnumerable<Type>)externalVisitorTypes);
        }

        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContainer"/> for code
        /// generation.
        /// </summary>
        /// <typeparam name="TExtra">
        /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
        /// </typeparam>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <param name="externalVisitorTypes">
        /// The types of concrete classes that implement <see cref="IRegistrationVisitor"/> from an external assembly.
        /// </param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static CompositionContainer Compose<TExtra>(
            this RegistrationSetup<TExtra> setup,
            IEnumerable<Type> externalVisitorTypes = null)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations =
                setup.Registrations.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<IRegistration>)kvp.Value.ToArray());
            return registrations.Compose(typeof(TExtra), typeof(ConstructionContext<TExtra>), externalVisitorTypes);
        }

        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContainer"/> for code
        /// generation.
        /// </summary>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <param name="externalVisitorTypes">
        /// The types of concrete classes that implement <see cref="IRegistrationVisitor"/> from an external assembly.
        /// </param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static CompositionContainer Compose(
            this RegistrationSetup setup,
            params Type[] externalVisitorTypes)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));
            if (externalVisitorTypes == null)
                throw new ArgumentNullException(nameof(externalVisitorTypes));

            return Compose(setup, (IEnumerable<Type>)externalVisitorTypes);
        }

        /// <summary>
        /// Composes the registration <paramref name="setup"/> into a <see cref="CompositionContainer"/> for code
        /// generation.
        /// </summary>
        /// <param name="setup">The registration <paramref name="setup"/>.</param>
        /// <param name="externalVisitorTypes">
        /// The types of concrete classes that implement <see cref="IRegistrationVisitor"/> from an external assembly.
        /// </param>
        /// <returns>The <see cref="CompositionContainer"/>.</returns>
        public static CompositionContainer Compose(
            this RegistrationSetup setup,
            IEnumerable<Type> externalVisitorTypes = null)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations =
                setup.Registrations.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<IRegistration>)kvp.Value.ToArray());
            return registrations.Compose(externalVisitorTypes: externalVisitorTypes);
        }

        private static CompositionContainer Compose(
            this IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations,
            Type extraDataType = null,
            Type constructionContextType = null,
            IEnumerable<Type> externalVisitorTypes = null)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));

            var context = new CompositionContainer(registrations, extraDataType, constructionContextType);

            ProcessRegistrations(context, externalVisitorTypes);

            // Get any registrations that have not been composed, e.g. an interface mapped to a class.
            // Only use single mappings (Count == 1), multiple mappings cannot be composed.
            IEnumerable<(Type serviceType, Type implementationType)> typeMappings =
                from kvp in registrations
                let regTypes = kvp.Value.DistinctRegistrationTypes().ToList()
                where !context.Compositions.ContainsKey(kvp.Key) && regTypes.Count == 1
                select (kvp.Key, regTypes[0]);

            // Re-reference the compositions under the type mappings.
            foreach ((Type serviceType, Type implementationType) in typeMappings)
            {
                context.AddComposition(serviceType, context.Compositions[implementationType]);
            }

            return context;
        }

        private static void ProcessRegistrations(
            CompositionContainer container,
            IEnumerable<Type> externalVisitorTypes = null)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            var visitorManager = new VisitorManager(container, externalVisitorTypes);

            IEnumerable<IRegistration> registrations = container.Registrations.Values.SelectMany(r => r);
            foreach (IRegistration registration in registrations)
            {
                visitorManager.Visit(registration);
            }
        }
    }
}
