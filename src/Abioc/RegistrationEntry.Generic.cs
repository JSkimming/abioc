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
    /// <typeparam name="TContructionContext">The type of the context used during service resolution.</typeparam>
    public class RegistrationEntry<TContructionContext>
        where TContructionContext : IContructionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationEntry{TContructionContext}"/> class.
        /// </summary>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedfactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        public RegistrationEntry(
            Type implementationType,
            Func<TContructionContext, object> factory = null,
            bool typedfactory = false)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            ImplementationType = implementationType;
            Factory = factory;
            Typedfactory = typedfactory;
        }

        /// <summary>
        /// Gets the implementation type of the <see cref="RegistrationEntry{TContructionContext}"/>.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets the factory for creating the <see cref="ImplementationType"/>; or <see langword="null"/> of there is
        /// no factory.
        /// </summary>
        public Func<TContructionContext, object> Factory { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Factory"/> is strongly typed.
        /// </summary>
        public bool Typedfactory { get; }
    }
}
