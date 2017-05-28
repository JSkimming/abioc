// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The construction context.
    /// </summary>
    /// <typeparam name="TExtra">The type of the <see cref="Extra"/> construction context information.</typeparam>
    public struct ConstructionContext<TExtra>
    {
        /// <summary>
        /// The default construction context to be used when one cannot be provided.
        /// </summary>
        public static readonly ConstructionContext<TExtra> Default =
            new ConstructionContext<TExtra>(typeof(void), typeof(void), typeof(void));

        /// <summary>
        /// Gets the <see cref="Type"/> of the service to be provided that satisfies the
        /// <see cref="ServiceType"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ImplementationType"/> should be assignable to the <see cref="ServiceType"/>
        /// </remarks>
        public readonly Type ImplementationType;

        /// <summary>
        /// Gets the <see cref="Type"/> of the requested service.
        /// </summary>
        /// <remarks>
        /// The <see cref="ServiceType"/> should be satisfied by being
        /// <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> the <see cref="ImplementationType"/>.
        /// </remarks>
        public readonly Type ServiceType;

        /// <summary>
        /// Gets the type of the component into which the service <see cref="ServiceType"/> is injected.
        /// </summary>
        public readonly Type RecipientType;

        /// <summary>
        /// Gets the <see cref="Extra"/> construction context information to be propitiated through all construction.
        /// </summary>
        public readonly TExtra Extra;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructionContext{TExtra}"/> struct.
        /// </summary>
        /// <param name="implementationType">The <see cref="ImplementationType"/>.</param>
        /// <param name="serviceType">The <see cref="ServiceType"/>.</param>
        /// <param name="recipientType">The <see cref="RecipientType"/>.</param>
        /// <param name="extra">The <see cref="Extra"/> construction context information.</param>
        public ConstructionContext(
            Type implementationType,
            Type serviceType,
            Type recipientType,
            TExtra extra = default(TExtra))
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
            Extra = extra;
        }
    }
}
