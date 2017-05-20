// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A composition to produce code to create a class via a constructor.
    /// </summary>
    public class ConstructorComposition : CompositionBase
    {
        private readonly List<IParameterExpression> _parameterExpressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorComposition"/> class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> created by the constructor.</param>
        /// <param name="parameters">The <see cref="Parameters"/> of the constructor.</param>
        /// <param name="isDefault">
        /// A value indicating whether this is the default composition, and therefore can be superseded by
        /// another composition.
        /// </param>
        public ConstructorComposition(
            Type type,
            IReadOnlyList<ParameterInfo> parameters,
            bool isDefault = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            Type = type;
            Parameters = parameters;
            IsDefault = isDefault;

            _parameterExpressions = new List<IParameterExpression>(parameters.Count);
        }

        /// <summary>
        /// Gets the type created by the constructor.
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Gets the parameters of the constructor.
        /// </summary>
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <summary>
        /// Gets a value indicating whether this is the default composition, and therefore can be superseded by
        /// another composition.
        /// </summary>
        public bool IsDefault { get; }

        /// <inheritdoc/>
        public override string GetInstanceExpression(CompositionContainer container, bool simpleName)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            // Get the expressions for the all the constructor parameters.
            IEnumerable<IParameterExpression> compositions = GetParameterExpressions(container);
            IEnumerable<string> parameterExpressions =
                compositions.Select(c => c.GetInstanceExpression(container, simpleName));

            // Join the parameters expressions.
            string parameters =
                string.Join(
                    "," + Environment.NewLine + "    ",
                    parameterExpressions.Select(p => CodeGen.Indent(p)));

            // Create the new Expression.
            string expression = string.IsNullOrEmpty(parameters)
                ? $"new {Type.ToCompileName()}()"
                : $"new {Type.ToCompileName()}({Environment.NewLine}    {parameters})";
            return expression;
        }

        /// <inheritdoc/>
        public override string GetComposeMethodName(CompositionContainer container, bool simpleName)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            string methodName = "Create" + Type.ToCompileMethodName(simpleName);
            return methodName;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetMethods(CompositionContainer container, bool simpleName)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            string parameter = RequiresConstructionContext(container)
                ? $"{Environment.NewLine}    {container.ConstructionContext} context"
                : string.Empty;

            string methodName = GetComposeMethodName(container, simpleName);
            string signature = $"private {Type.ToCompileName()} {methodName}({parameter})";

            string instanceExpression = GetInstanceExpression(container, simpleName);
            instanceExpression = CodeGen.Indent(instanceExpression);

            string method =
                string.Format("{0}{2}{{{2}    return {1};{2}}}", signature, instanceExpression, Environment.NewLine);
            return new[] { method };
        }

        /// <inheritdoc/>
        public override bool RequiresConstructionContext(CompositionContainer container)
        {
            return GetParameterExpressions(container).Any(c => c.RequiresConstructionContext(container));
        }

        private IEnumerable<IParameterExpression> GetParameterExpressions(CompositionContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (_parameterExpressions.Count == Parameters.Count)
                return _parameterExpressions;

            foreach (ParameterInfo parameter in Parameters)
            {
                if (container.Compositions.TryGetValue(parameter.ParameterType, out IComposition composition))
                {
                    IParameterExpression expression = new SimpleParameterExpression(composition);
                    _parameterExpressions.Add(expression);
                    continue;
                }

                TypeInfo parameterTypeInfo = parameter.ParameterType.GetTypeInfo();
                if (parameterTypeInfo.IsGenericType)
                {
                    Type genericTypeDefinition = parameterTypeInfo.GetGenericTypeDefinition();
                    if (typeof(IEnumerable<>) == genericTypeDefinition)
                    {
                        Type enumerableType = parameterTypeInfo.GenericTypeArguments.Single();
                        IParameterExpression expression =
                            new EnumerableParameterExpression(enumerableType, container.ConstructionContext.Length > 0);
                        _parameterExpressions.Add(expression);
                        continue;
                    }
                }

                string message =
                    $"Failed to get the compositions for the parameter '{parameter}' to the constructor of " +
                    $"'{Type}'. Is there a missing registration mapping?";
                throw new CompositionException(message);
            }

            return _parameterExpressions;
        }
    }
}
