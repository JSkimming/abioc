// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods on <see cref="CompilationContext{TContructionContext}"/>.
    /// </summary>
    public static class CompilationContextExtensions
    {
        /// <summary>
        /// Gets any services that are defined in the <see cref="CompilationContext{T}.MultiMappings"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="context">The compilation context.</param>
        /// <returns>
        /// Any services that are defined in the <see cref="CompilationContext{T}.MultiMappings"/> for the
        /// <typeparamref name="TService"/>.
        /// </returns>
        public static IEnumerable<TService> GetServices<TService>(
            this CompilationContext<DefaultContructionContext> context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.GetServices<TService>(new DefaultContructionContext());
        }

        /// <summary>
        /// Gets the service that is defined in the <see cref="CompilationContext{T}.SingleMappings"/> for the
        /// <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to get.</typeparam>
        /// <param name="context">The compilation context.</param>
        /// <returns>
        /// The service that is defined in the <see cref="CompilationContext{T}.SingleMappings"/> for the
        /// <typeparamref name="TService"/>.
        /// </returns>
        /// <exception cref="DiException">There are no mappings for the <typeparamref name="TService"/>.</exception>
        public static TService GetService<TService>(this CompilationContext<DefaultContructionContext> context)
        {
            return context.GetService<TService>(new DefaultContructionContext());
        }
    }
}
