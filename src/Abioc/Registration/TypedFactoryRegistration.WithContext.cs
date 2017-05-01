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
    /// <typeparamref name="TImplementation"/> through a factory function.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    /// <typeparam name="TImplementation">The <see cref="IRegistration.ImplementationType"/>.</typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TypedFactoryRegistration<TExtra, TImplementation> : RegistrationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedFactoryRegistration{TExtra, ImplementationType}"/> class.
        /// </summary>
        /// <param name="factory">
        /// The factory function that produces services of type <typeparamref name="TImplementation"/>.
        /// </param>
        public TypedFactoryRegistration(Func<ConstructionContext<TExtra>, TImplementation> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            Factory = factory;
        }

        /// <summary>
        /// Gets the <see cref="IRegistration.ImplementationType"/> of the <see cref="IRegistration"/>.
        /// </summary>
        public override Type ImplementationType => typeof(TImplementation);

        /// <summary>
        /// Gets the
        /// </summary>
        public Func<ConstructionContext<TExtra>, TImplementation> Factory { get; }

        private string DebuggerDisplay =>
            $"{typeof(TypedFactoryRegistration<,>).Name}: Type={ImplementationType.Name}";
    }
}
