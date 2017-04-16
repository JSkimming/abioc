// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Static helper methods for <see cref="RegistrationEntry{TContructionContext}"/>.
    /// </summary>
    public static class RegistrationEntry
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RegistrationEntry{TContructionContext}"/> class.
        /// </summary>
        /// <typeparam name="TContructionContext">The type of the context used during service resolution.</typeparam>
        /// <param name="implementationType">The type of the implemented service.</param>
        /// <param name="factory">
        /// The factory function that produces services of type <paramref name="implementationType"/>. If not specified
        /// the an instance of <paramref name="implementationType"/> will be automatically generated.
        /// </param>
        /// <param name="typedfactory">
        /// A value indicating whether the <paramref name="factory"/> is strongly typed.
        /// </param>
        /// <returns>A new instance of the <see cref="RegistrationEntry{TContructionContext}"/> class.</returns>
        public static RegistrationEntry<TContructionContext> Create<TContructionContext>(
            Type implementationType,
            Func<TContructionContext, object> factory = null,
            bool typedfactory = false)
            where TContructionContext : IContructionContext
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            return new RegistrationEntry<TContructionContext>(implementationType, factory, typedfactory);
        }
    }
}
