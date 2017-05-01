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

        /// <summary>
        /// Gets or sets a value indicating whether a registration is internal; e.g. it is required as a dependency for
        /// other registrations but will not be resolved externally via a call to
        /// <see cref="AbiocContainer.GetService"/> or <see cref="AbiocContainer.GetServices"/>.
        /// </summary>
        /// <remarks>
        /// This is simple performance optimization to prevent unnecessary dependency injection type mappings from
        /// polluting the resolution and therefore slowing it down.
        /// </remarks>
        bool Internal { get; set; }
    }
}
