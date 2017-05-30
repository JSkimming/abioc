// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;

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

        /// <summary>
        /// Gets the service of type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <returns>The service of type <typeparamref name="TService"/>.</returns>
        TService GetService<TService>();

        /// <summary>
        /// Gets any services of the type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the services to get.</param>
        /// <returns>Any services of the <paramref name="serviceType"/>.</returns>
        IEnumerable<object> GetServices(Type serviceType);

        /// <summary>
        /// Gets any services of the type <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <returns>Any services of the type  <typeparamref name="TService"/>.</returns>
        IEnumerable<TService> GetServices<TService>();
    }
}
