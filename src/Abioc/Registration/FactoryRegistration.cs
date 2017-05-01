// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A <see cref="IRegistration"/> entry that produces the code to provided services of type
    /// <see cref="IRegistration.ImplementationType"/> through a factory function.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FactoryRegistration : RegistrationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryRegistration"/> class.
        /// </summary>
        /// <param name="implementationType">
        /// The <see cref="IRegistration.ImplementationType"/> type of the <see cref="IRegistration"/>.
        /// </param>
        /// <param name="factory">
        /// The factory function that produces services of type <see cref="IRegistration.ImplementationType"/>.
        /// </param>
        public FactoryRegistration(Type implementationType, Func<object> factory)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            ImplementationType = implementationType;
            Factory = factory;
        }

        /// <summary>
        /// Gets the <see cref="IRegistration.ImplementationType"/> of the <see cref="IRegistration"/>.
        /// </summary>
        public override Type ImplementationType { get; }

        /// <summary>
        /// Gets the
        /// </summary>
        public Func<object> Factory { get; }

        private string DebuggerDisplay => $"{GetType().Name}: Type={ImplementationType.Name}";
    }
}
