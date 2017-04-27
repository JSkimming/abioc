// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An entry for a registration mapping.
    /// </summary>
    internal class RegistrationEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationEntry"/> class.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedFactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        /// <param name="factoryRequiresContext">
        /// A value indicating whether the <see cref="Factory"/> takes a construction context.
        /// </param>
        public RegistrationEntry(
            Type implementationType,
            object factory = null,
            bool typedFactory = false,
            bool factoryRequiresContext = false)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            ImplementationType = implementationType;
            Factory = factory;
            TypedFactory = typedFactory;
            FactoryRequiresContext = factoryRequiresContext;
        }

        /// <summary>
        /// Gets the implementation type of the <see cref="RegistrationEntry"/>.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets the factory for creating the <see cref="ImplementationType"/>; or <see langword="null"/> of there is
        /// no factory.
        /// </summary>
        public object Factory { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Factory"/> is strongly typed.
        /// </summary>
        public bool TypedFactory { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Factory"/> takes a construction context.
        /// </summary>
        public bool FactoryRequiresContext { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="RegistrationEntry"/> class.
        /// </summary>
        /// <typeparam name="TConstructionContext">The type of the context used during service resolution.</typeparam>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedfactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        /// <returns>A new instance of the <see cref="RegistrationEntry"/> class.</returns>
        public static RegistrationEntry Create<TConstructionContext>(
            Type implementationType,
            Func<TConstructionContext, object> factory = null,
            bool typedfactory = false)
            where TConstructionContext : IConstructionContext
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            return new RegistrationEntry(implementationType, factory, typedfactory, factoryRequiresContext: true);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RegistrationEntry"/> class.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedfactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        /// <returns>A new instance of the <see cref="RegistrationEntry"/> class.</returns>
        public static RegistrationEntry Create(
            Type implementationType,
            Func<object> factory = null,
            bool typedfactory = false)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            return new RegistrationEntry(implementationType, factory, typedfactory, factoryRequiresContext: false);
        }
    }
}
