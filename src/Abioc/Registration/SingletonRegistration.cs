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
    public class SingletonRegistration : IRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonRegistration"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="Inner"/> <see cref="IRegistration"/>.</param>
        public SingletonRegistration(IRegistration inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            Inner = inner;
        }

        /// <summary>
        /// Gets the <see cref="IRegistration.ImplementationType"/> of the <see cref="IRegistration"/>.
        /// </summary>
        public Type ImplementationType => Inner.ImplementationType;

        /// <summary>
        /// Gets the <see cref="Inner"/> <see cref="IRegistration"/>.
        /// </summary>
        public IRegistration Inner { get; }
    }
}
