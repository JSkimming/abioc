// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// The simplest creation registration, that will produce a factory function to create a class of type
    /// <see cref="IRegistration.ImplementationType"/> using a single public constructor.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SingleConstructorRegistration : IRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleConstructorRegistration"/> class.
        /// </summary>
        /// <param name="implementationType">
        /// The <see cref="IRegistration.ImplementationType"/> type of the <see cref="IRegistration"/>.
        /// </param>
        public SingleConstructorRegistration(Type implementationType)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            ImplementationType = implementationType;
        }

        /// <summary>
        /// Gets the <see cref="IRegistration.ImplementationType"/> of the <see cref="IRegistration"/>.
        /// </summary>
        public Type ImplementationType { get; }

        private string DebuggerDisplay => $"{GetType().Name}: Type={ImplementationType.Name}";
    }
}
