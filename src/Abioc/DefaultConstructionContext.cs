// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The default <see cref="IConstructionContext"/>.
    /// </summary>
    /// <remarks>
    /// This class used when a custom construction is not required, or it can be the base class for a custom
    /// construction context.
    /// </remarks>
    public class DefaultConstructionContext : IConstructionContext
    {
        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the service to be provided that satisfies the
        /// <see cref="ServiceType"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ImplementationType"/> should be assignable to the <see cref="ServiceType"/>
        /// </remarks>
        public Type ImplementationType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the requested service.
        /// </summary>
        /// <remarks>
        /// The <see cref="ServiceType"/> should be satisfied by being
        /// <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the <see cref="ImplementationType"/>.
        /// </remarks>
        public Type ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the type of the component into which the service <see cref="ServiceType"/> is injected.
        /// </summary>
        public Type RecipientType { get; set; }
    }
}
