// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The initialization interface implemented by the generated class for the dependency injection container.
    /// </summary>
    public interface IContainerInitialization : IContainer
    {
        /// <summary>
        /// Returns the dictionary of types to factory functions.
        /// </summary>
        /// <returns>The dictionary of types to factory functions.</returns>
        Dictionary<Type, Func<object>> GetCreateMap();
    }
}
