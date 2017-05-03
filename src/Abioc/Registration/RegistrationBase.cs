// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A base class that provides a default implementation of a <see cref="IRegistration"/>.
    /// </summary>
    public abstract class RegistrationBase : IRegistration
    {
        /// <inheritdoc />
        public abstract Type ImplementationType { get; }

        /// <inheritdoc />
        public virtual bool Internal { get; set; }
    }
}
