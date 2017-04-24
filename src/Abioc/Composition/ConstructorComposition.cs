// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
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
        /// <param name="type">The <see cref="Type"/> created by the <paramref name="constructor"/>.</param>
        /// <param name="constructor">The information about the <see cref="Constructor"/>.</param>
        /// <param name="parameters">The <see cref="Parameters"/> of the <paramref name="constructor"/>.</param>
        public ConstructorComposition(
            Type type,
            ConstructorInfo constructor,
            IReadOnlyList<ParameterInfo> parameters)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            Type = type;
            Constructor = constructor;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the type created by the <see cref="Constructor"/>.
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Gets the information about the <see cref="Constructor"/>.
        /// </summary>
        public ConstructorInfo Constructor { get; }

        /// <summary>
        /// Gets the parameters of the <see cref="Constructor"/>.
        /// </summary>
        public IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <inheritdoc/>
        public override IEnumerable<string> GetMethods(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string parameter = RequiresContructionContext(context)
                ? $"{Environment.NewLine}    {context.ContructionContext} context"
                : string.Empty;

            string methodName = "Create_" + Type.ToCompileMethodName();
            string signature = $"private static {Type.ToCompileName()} {methodName}({parameter})";

            string instanceExpression = GetInstanceExpression(context);
            instanceExpression = Indent(instanceExpression);

            string method =
                string.Format("{0}{2}{{{2}    return {1};{2}}}", signature, instanceExpression, Environment.NewLine);
            return new[] { method };
        }

        /// <inheritdoc/>
        public override bool RequiresContructionContext(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            try
            {
                IEnumerable<IComposition> compositions =
                    GetCompositions(context, Parameters.Select(p => p.ParameterType));

                return compositions.Any(c => c.RequiresContructionContext(context));
            }
            catch (Exception ex)
            {
                string message = "Failed to get the compositions for the parameters to the constructor of " +
                                 $"'{Type}'. Is there a missing registration mapping?";
                throw new CompositionException(message, ex);
            }
        }

        /// <inheritdoc/>
        public override string GetInstanceExpression(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            try
            {
                // Get the expressions for the all the constructor parameters.
                IEnumerable<IComposition> compositions =
                    GetCompositions(context, Parameters.Select(p => p.ParameterType));
                IEnumerable<string> parameterExpressions = compositions.Select(c => c.GetInstanceExpression(context));

                // Join the parameters expressions.
                string parameters =
                    string.Join(
                        "," + Environment.NewLine + "    ",
                        parameterExpressions.Select(p => Indent(p)));

                // Create the new Expression.
                string expression = string.IsNullOrEmpty(parameters)
                    ? $"new {Type.ToCompileName()}()"
                    : $"new {Type.ToCompileName()}({Environment.NewLine}    {parameters})";
                return expression;
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
