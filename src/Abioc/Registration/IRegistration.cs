// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The interface that all registrations must implement.
    /// </summary>
    public interface IRegistration
    {
        /// <summary>
        /// Gets the <see cref="IRegistration.ImplementationType"/> of the <see cref="IRegistration"/>.
        /// </summary>
        Type ImplementationType { get; }
    }
}
