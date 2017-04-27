// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition;

    /// <summary>
    /// A composition to produce code to create a factory field.
    /// </summary>
    public class FactoryComposition : CompositionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryComposition"/> class.
        /// </summary>
        /// <param name="type">The type of instance produced by the <paramref name="factory"/>.</param>
        /// <param name="factory">The factory function that produces services of the <paramref name="type"/>.</param>
        /// <param name="constructionContextType">
        /// The type of the <see cref="ConstructionContext{T}"/> required by the <paramref name="factory"/>, if
        /// required.
        /// </param>
        public FactoryComposition(
            Type type,
            object factory,
            Type constructionContextType = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            Type = type;
            Factory = factory;
            ConstructionContextType = constructionContextType;
        }

        /// <summary>
        /// Gets the type of instance produced by the <see cref="Factory"/>.
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Gets the factory function that produces services of the <see cref="Type"/>.
        /// </summary>
        public object Factory { get; }

        /// <summary>
        /// Gets the type of the <see cref="ConstructionContext{T}"/> required by the <see cref="Factory"/>.
        /// <see langword="null"/> indicates the <see cref="Factory"/> does not require a
        /// <see cref="ConstructionContext{T}"/>.
        /// </summary>
        public Type ConstructionContextType { get; }

        /// <inheritdoc/>
        public override string GetComposeMethodName(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = GetFactoryFieldName();
            return methodName;
        }

        /// <inheritdoc />
        public override string GetInstanceExpression(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string factoryFieldName = GetFactoryFieldName();

            string instanceExpression =
                RequiresConstructionContext()
                    ? factoryFieldName + "(context)"
                    : factoryFieldName + "()";

            return instanceExpression;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetMethods(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public override IEnumerable<(string code, object value)> GetFieldInitializations(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string fieldName = GetFactoryFieldName();
            string returnType = Type.ToCompileName();
            string fieldType =
                RequiresConstructionContext()
                    ? $"System.Func<{ConstructionContextType.ToCompileName()}, {returnType}>"
                    : $"System.Func<{returnType}>";

            string code = $"{fieldName} = ({fieldType})";
            return new[] { (code, Factory) };
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetFields(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string fieldName = GetFactoryFieldName();
            string returnType = Type.ToCompileName();
            string fieldType =
                RequiresConstructionContext()
                    ? $"System.Func<{ConstructionContextType.ToCompileName()}, {returnType}>"
                    : $"System.Func<{returnType}>";

            string field = $"private static {fieldType} {fieldName};";
            return new[] { field };
        }

        /// <inheritdoc/>
        public override bool RequiresConstructionContext(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return RequiresConstructionContext();
        }

        private bool RequiresConstructionContext() => ConstructionContextType != null;

        private string GetFactoryFieldName() => "Factor_" + Type.ToCompileMethodName(simpleName: false);
    }
}
