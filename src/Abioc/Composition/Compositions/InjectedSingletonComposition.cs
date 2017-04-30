// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A composition to produce code for a injected value.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the injected <see cref="Value"/>.</typeparam>
    public class InjectedSingletonComposition<TImplementation> : CompositionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectedSingletonComposition{TImplementation}"/> class.
        /// </summary>
        /// <param name="value">
        /// The <see cref="Value"/> of type <typeparamref name="TImplementation"/>.
        /// </param>
        public InjectedSingletonComposition(TImplementation value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the type of the <see cref="Value"/>.
        /// </summary>
        public override Type Type => typeof(TImplementation);

        /// <summary>
        /// Gets the injected value.
        /// </summary>
        public TImplementation Value { get; }

        /// <inheritdoc/>
        public override string GetComposeMethodName(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = "Accessor" + Type.ToCompileMethodName(simpleName);
            return methodName;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetMethods(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = GetComposeMethodName(context, simpleName);
            string instanceExpression = GetInstanceExpression(context, simpleName);

            string method =
                Type.GetTypeInfo().IsValueType
                    ? string.Format(
                        "private static object {0}(){1}{{{1}    return (object){2};{1}}}",
                        methodName,
                        Environment.NewLine,
                        instanceExpression)
                    : string.Format(
                        "private static {0} {1}(){2}{{{2}    return {3};{2}}}",
                        Type.ToCompileName(),
                        methodName,
                        Environment.NewLine,
                        instanceExpression);

            return new[] { method };
        }

        /// <inheritdoc />
        public override string GetInstanceExpression(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string injectedFieldName = GetInjectedFieldName();
            return injectedFieldName;
        }

        /// <inheritdoc />
        public override IEnumerable<(string snippet, object value)> GetFieldInitializations(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string fieldName = GetInjectedFieldName();
            string fieldType = Type.ToCompileName();

            string snippet = $"{fieldName} = ({fieldType})";
            return new[] { (snippet, (object)Value) };
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetFields(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string fieldName = GetInjectedFieldName();
            string fieldType = Type.ToCompileName();

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

        private bool RequiresConstructionContext() => false;

        private string GetInjectedFieldName() => "Injected_" + Type.ToCompileMethodName(simpleName: false);
    }
}
