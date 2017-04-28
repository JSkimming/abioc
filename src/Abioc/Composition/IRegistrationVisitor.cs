// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Registration;

    /// <summary>
    /// The interface implemented by visitors of a <see cref="IRegistration"/>.
    /// </summary>
    public interface IRegistrationVisitor
    {
        /// <summary>
        /// Initializes the <see cref="IRegistrationVisitor"/>.
        /// </summary>
        /// <param name="context">The composition context.</param>
        void Initialize(CompositionContext context);
    }
}
