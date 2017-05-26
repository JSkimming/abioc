// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension helper methods on <see cref="IGenerationContext"/>.
    /// </summary>
    public static class GenerationContextExtensions
    {
        /// <summary>
        /// Creates a customized <see cref="IGenerationContext"/> where the
        /// <see cref="IGenerationContext.ConstructionContextDefinition"/> is updated with the specified parameters.
        /// </summary>
        /// <param name="context">The <see cref="IGenerationContext"/> to customize.</param>
        /// <param name="implementationType">
        /// The <see cref="ConstructionContextDefinition.ImplementationType"/>.
        /// </param>
        /// <param name="serviceType">The <see cref="ConstructionContextDefinition.ServiceType"/>.</param>
        /// <param name="recipientType">The <see cref="ConstructionContextDefinition.RecipientType"/>.</param>
        /// <returns>
        /// A customized <see cref="IGenerationContext"/> where the
        /// <see cref="IGenerationContext.ConstructionContextDefinition"/> is updated with the specified parameters.
        /// </returns>
        public static IGenerationContext Customize(
            this IGenerationContext context,
            Type implementationType = null,
            Type serviceType = null,
            Type recipientType = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            implementationType = implementationType ?? context.ConstructionContextDefinition.ImplementationType;
            serviceType = serviceType ?? context.ConstructionContextDefinition.ServiceType;
            recipientType = recipientType ?? context.ConstructionContextDefinition.RecipientType;

            var definition = new ConstructionContextDefinition(implementationType, serviceType, recipientType);
            IGenerationContext customization = context.Customize(definition);
            return customization;
        }
    }
}
