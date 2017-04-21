// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The registration setup for registrations that require a <see cref="ContructionContext{TExtra}"/>.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ContructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    public class RegistrationSetup<TExtra> : RegistrationSetupBase<RegistrationSetup<TExtra>>
    {
    }
}
