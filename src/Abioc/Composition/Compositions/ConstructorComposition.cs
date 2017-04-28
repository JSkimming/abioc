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
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorComposition"/> class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> created by the constructor.</param>
        /// <param name="parameters">The <see cref="Parameters"/> of the constructor.</param>
        public ConstructorComposition(
            Type type,
            IReadOnlyList<ParameterInfo> parameters)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            Type = type;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the type created by the constructor.
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Gets the parameters of the constructor.
        /// </summary>
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <inheritdoc/>
        public override string GetInstanceExpression(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Get the expressions for the all the constructor parameters.
            IReadOnlyList<IComposition> compositions = GetParameterCompositions(context);
            IEnumerable<string> parameterExpressions =
                compositions.Select(c => c.GetInstanceExpression(context, simpleName));

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
        public override string GetComposeMethodName(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = "Create" + Type.ToCompileMethodName(simpleName);
            return methodName;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetMethods(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string parameter = RequiresConstructionContext(context)
                ? $"{Environment.NewLine}    {context.ConstructionContext} context"
                : string.Empty;

            string methodName = GetComposeMethodName(context, simpleName);
            string signature = $"private static {Type.ToCompileName()} {methodName}({parameter})";

            string instanceExpression = GetInstanceExpression(context, simpleName);
            instanceExpression = CodeGen.Indent(instanceExpression);

            string method =
                string.Format("{0}{2}{{{2}    return {1};{2}}}", signature, instanceExpression, Environment.NewLine);
            return new[] { method };
        }

        /// <inheritdoc/>
        public override bool RequiresConstructionContext(CompositionContext context)
        {
            return GetParameterCompositions(context).Any(c => c.RequiresConstructionContext(context));
        }

        private IReadOnlyList<IComposition> GetParameterCompositions(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            try
            {
                return GetCompositions(context, Parameters.Select(p => p.ParameterType)).ToList();
            }
            catch (Exception ex)
            {
                string message = "Failed to get the compositions for the parameters to the constructor of " +
                                 $"'{Type}'. Is there a missing registration mapping?";
                throw new CompositionException(message, ex);
            }
        }
    }
}
