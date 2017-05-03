// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;

    /// <summary>
    /// The interface implemented by the generated class for the dependency injection container.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Gets the service of type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>The service of type <paramref name="serviceType"/>.</returns>
        object GetService(Type serviceType);
    }
}
