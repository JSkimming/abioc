// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The interface that must be implemented by a custom construction context.
    /// </summary>
    /// <remarks>
    /// A construction context is a mechanism whereby data is passed through the service resolution process allowing
    /// custom information be provided, e.g. a logging context can be provided for particular process or request.
    /// </remarks>
    public interface IContructionContext
    {
        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the service to be provided that satisfies the
        /// <see cref="ServiceType"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ImplementationType"/> should be assignable to the <see cref="ServiceType"/>
        /// </remarks>
        Type ImplementationType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the requested service.
        /// </summary>
        /// <remarks>
        /// The <see cref="ServiceType"/> should be satisfied by being
        /// <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the <see cref="ImplementationType"/>.
        /// </remarks>
        Type ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the type of the component into which the service <see cref="ServiceType"/> is injected.
        /// </summary>
        Type RecipientType { get; set; }
    }
}
