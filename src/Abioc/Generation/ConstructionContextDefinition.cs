// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines the requirements of a <see cref="ConstructionContext{T}"/> during composition.
    /// </summary>
    public class ConstructionContextDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructionContextDefinition"/> class.
        /// </summary>
        /// <param name="implementationType">The <see cref="ImplementationType"/>.</param>
        /// <param name="serviceType">The <see cref="ServiceType"/>.</param>
        /// <param name="recipientType">The <see cref="RecipientType"/>.</param>
        public ConstructionContextDefinition(
            Type implementationType,
            Type serviceType,
            Type recipientType)
        {
            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (recipientType == null)
                throw new ArgumentNullException(nameof(recipientType));

            ImplementationType = implementationType;
            ServiceType = serviceType;
            RecipientType = recipientType;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of the service to be provided that satisfies the <see cref="ServiceType"/>.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the requested service.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the type of the component into which the service <see cref="ServiceType"/> is injected.
        /// </summary>
        public Type RecipientType { get; }
    }
}
