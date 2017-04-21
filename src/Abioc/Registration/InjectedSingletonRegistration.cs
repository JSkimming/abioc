// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A <see cref="IRegistration"/> entry that produces the code to use a injected constant value.
    /// </summary>
    /// <typeparam name="TImplementation">The <see cref="IRegistration.ImplementationType"/></typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InjectedSingletonRegistration<TImplementation> : IRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectedSingletonRegistration{TImplementation}"/> class.
        /// </summary>
        /// <param name="value">
        /// The <see cref="Value"/> of type <typeparamref name="TImplementation"/>.
        /// </param>
        public InjectedSingletonRegistration(TImplementation value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the <see cref="IRegistration.ImplementationType"/> of the <see cref="IRegistration"/>.
        /// </summary>
        public Type ImplementationType => typeof(TImplementation);

        /// <summary>
        /// Gets the constant value.
        /// </summary>
        public TImplementation Value { get; }

        private string DebuggerDisplay =>
            $"{typeof(InjectedSingletonRegistration<>).Name}: Type={ImplementationType.Name}";
    }
}
