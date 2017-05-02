// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;

    /// <summary>
    /// The interface implemented by the generated class for the dependency injection container.
    /// </summary>
    /// <typeparam name="TExtra">
    /// The type of the <see cref="ConstructionContext{TExtra}.Extra"/> construction context information.
    /// </typeparam>
    public interface IContainer<in TExtra>
    {
        /// <summary>
        /// Gets the service of type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <param name="extraData">The custom extra data used during construction.</param>
        /// <returns>The service of type <paramref name="serviceType"/>.</returns>
        object GetService(Type serviceType, TExtra extraData);
    }
}
