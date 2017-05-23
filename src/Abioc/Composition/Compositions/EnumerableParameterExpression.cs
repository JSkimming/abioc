// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Generation;

    /// <summary>
    /// The parameter expression that uses <see cref="IContainer.GetServices"/> to resolve a parameter.
    /// </summary>
    internal class EnumerableParameterExpression : IParameterExpression
    {
        private readonly bool _requiresConstructionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableParameterExpression"/> class.
        /// </summary>
        /// <param name="enumerableType">The type of the <see cref="IEnumerable{T}"/> parameter.</param>
        /// <param name="requiresConstructionContext">
        /// The value indicating whether the <see cref="IParameterExpression"/> requires a
        /// <see cref="ConstructionContext{T}"/>.
        /// </param>
        public EnumerableParameterExpression(Type enumerableType, bool requiresConstructionContext)
        {
            if (enumerableType == null)
                throw new ArgumentNullException(nameof(enumerableType));

            EnumerableType = enumerableType;
            _requiresConstructionContext = requiresConstructionContext;
        }

        /// <summary>
        /// Gets the type of the <see cref="IEnumerable{T}"/> <see cref="IParameterExpression"/>.
        /// </summary>
        public Type EnumerableType { get; }

        /// <inheritdoc />
        public string GetInstanceExpression(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string enumerableTypeName = EnumerableType.ToCompileName();
            string serviceTypeparameter = $"typeof({enumerableTypeName})";
            string contextParameter = _requiresConstructionContext ? ", context.Extra" : string.Empty;

            string expression = $"GetServices({serviceTypeparameter}{contextParameter}).Cast<{enumerableTypeName}>()";
            return expression;
        }

        /// <inheritdoc />
        public bool RequiresConstructionContext(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return _requiresConstructionContext;
        }
    }
}
