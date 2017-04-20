// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The context to maintain the meta-data for an IoC context.
    /// </summary>
    public abstract class RegistrationContextBase
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        internal Dictionary<Type, List<RegistrationEntry>> Context { get; }
            = new Dictionary<Type, List<RegistrationEntry>>(32);
    }
}
