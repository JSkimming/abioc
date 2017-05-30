// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Generation;

    /// <summary>
    /// A composition to produce code for property dependency injection.
    /// </summary>
    internal class PropertyDependencyComposition : IComposition
    {
        private static readonly string NewLine = Environment.NewLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDependencyComposition"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="Inner"/> <see cref="IComposition"/>.</param>
        /// <param name="propertiesToInject">The list of <see cref="PropertiesToInject"/>.</param>
        public PropertyDependencyComposition(
            IComposition inner,
            (string property, Type type)[] propertiesToInject)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (propertiesToInject == null)
                throw new ArgumentNullException(nameof(propertiesToInject));

            Inner = inner;
            PropertiesToInject = propertiesToInject;
        }

        /// <summary>
        /// Gets the <see cref="Inner"/> <see cref="IComposition"/>.
        /// </summary>
        public IComposition Inner { get; }

        /// <summary>
        /// Gets the type provided by the <see cref="Inner"/> <see cref="IComposition"/>.
        /// </summary>
        public Type Type => Inner.Type;

        /// <summary>
        /// Gets the list of properties to inject.
        /// </summary>
        public (string property, Type type)[] PropertiesToInject { get; }

        /// <inheritdoc />
        public string GetInstanceExpression(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = GetComposeMethodName(context);
            string parameter = RequiresConstructionContext(context) ? "context" : string.Empty;

            string expression = $"{methodName}({parameter})";
            return expression;
        }

        /// <inheritdoc />
        public string GetComposeMethodName(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = "PropertyInjection" + Type.ToCompileMethodName(context.UsingSimpleNames);
            return methodName;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetMethods(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IEnumerable<string> innerMethods = Inner.GetMethods(context);
            foreach (string innerMethod in innerMethods)
            {
                yield return innerMethod;
            }

            string parameter = RequiresConstructionContext(context)
                ? $"{NewLine}    {context.ConstructionContext} context"
                : string.Empty;

            string methodName = GetComposeMethodName(context);
            string signature = $"private {Type.ToCompileName()} {methodName}({parameter})";

            string instanceExpression = Inner.GetInstanceExpression(context);
            instanceExpression = $"{NewLine}{Type.ToCompileName()} instance = {instanceExpression};";
            instanceExpression = CodeGen.Indent(instanceExpression);

            IEnumerable<string> propertyExpressions =
                from pe in GetPropertyExpressions(context)
                let ctx = context.Customize(recipientType: Type, serviceType: pe.expression.Type)
                select $"instance.{pe.property} = {pe.expression.GetInstanceExpression(ctx)};";

            string propertySetters = NewLine + string.Join(NewLine, propertyExpressions);
            propertySetters = CodeGen.Indent(propertySetters);

            string method = string.Format(
                "{0}{3}{{{1}{2}{3}    return instance;{3}}}",
                signature,
                instanceExpression,
                propertySetters,
                NewLine);
            yield return method;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetFields(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetFields(context);
        }

        /// <inheritdoc />
        public IEnumerable<(string snippet, object value)> GetFieldInitializations(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetFieldInitializations(context);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAdditionalInitializations(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetAdditionalInitializations(context);
        }

        /// <inheritdoc />
        public bool RequiresConstructionContext(IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.RequiresConstructionContext(context) ||
                   GetPropertyExpressions(context).Any(p => p.expression.RequiresConstructionContext(context));
        }

        private IEnumerable<(string property, IParameterExpression expression)> GetPropertyExpressions(
            IGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            foreach ((string property, Type type) in PropertiesToInject)
            {
                if (context.Compositions.TryGetValue(type, out IComposition composition))
                {
                    IParameterExpression expression = new SimpleParameterExpression(composition);
                    yield return (property, expression);
                    continue;
                }

                TypeInfo propertyTypeInfo = type.GetTypeInfo();
                if (propertyTypeInfo.IsGenericType)
                {
                    Type genericTypeDefinition = propertyTypeInfo.GetGenericTypeDefinition();
                    if (typeof(IEnumerable<>) == genericTypeDefinition)
                    {
                        Type enumerableType = propertyTypeInfo.GenericTypeArguments.Single();
                        IParameterExpression expression =
                            new EnumerableParameterExpression(enumerableType, context.HasConstructionContext);
                        yield return (property, expression);
                        continue;
                    }
                }

                string message =
                    $"Failed to get the composition for the property '{type.Name} {property}' of the instance " +
                    $"'{Type}'. Is there a missing registration mapping?";
                throw new CompositionException(message);
            }
        }
    }
}
