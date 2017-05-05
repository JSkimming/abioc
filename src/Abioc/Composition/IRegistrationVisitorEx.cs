// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extra interface for visitors.
    /// </summary>
    internal interface IRegistrationVisitorEx : IRegistrationVisitor
    {
        /// <summary>
        /// Initializes the <see cref="IRegistrationVisitor"/>.
        /// </summary>
        /// <param name="manager">The visitor manager.</param>
        void InitializeEx(VisitorManager manager);
    }
}
