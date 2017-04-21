// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Base class for a registration set-up.
    /// </summary>
    /// <typeparam name="TDerived">The type of the set-up derived from this class.</typeparam>
    public abstract class RegistrationSetupBase<TDerived>
        where TDerived : RegistrationSetupBase<TDerived>
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        public Dictionary<Type, List<IRegistration>> Context { get; }
            = new Dictionary<Type, List<IRegistration>>(32);

        /// <summary>
        /// Registers an <paramref name="entry"/> for generation with the registration <see cref="Context"/>.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service to by satisfied during registration. The <paramref name="serviceType"/> should be
        /// satisfied by being <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the
        /// <paramref name="entry"/>.<see cref="IRegistration.ImplementationType"/>
        /// </param>
        /// <param name="entry">The entry to be registered.</param>
        /// <returns><see langword="this"/> context to be used in a fluent configuration.</returns>
        public TDerived Register(Type serviceType, IRegistration entry)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            List<IRegistration> factories;
            if (!Context.TryGetValue(serviceType, out factories))
            {
                factories = new List<IRegistration>(1);
                Context[serviceType] = factories;
            }

            factories.Add(entry);

            return (TDerived)this;
        }
    }
}
