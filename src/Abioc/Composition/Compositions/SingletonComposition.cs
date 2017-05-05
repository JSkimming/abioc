// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition.Compositions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A composition to produce code for a singleton value.
    /// </summary>
    public class SingletonComposition : IComposition
    {
        private static readonly string NewLine = Environment.NewLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonComposition"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="Inner"/> <see cref="IComposition"/>.</param>
        public SingletonComposition(IComposition inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            Inner = inner;
        }

        /// <summary>
        /// Gets the <see cref="Inner"/> <see cref="IComposition"/>.
        /// </summary>
        public IComposition Inner { get; }

        /// <summary>
        /// Gets the type provided by the <see cref="Inner"/> <see cref="IComposition"/>.
        /// </summary>
        public Type Type => Inner.Type;

        /// <inheritdoc />
        public string GetInstanceExpression(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string fieldName = GetLazyFieldName();
            string instanceExpression = fieldName + ".Value";
            return instanceExpression;
        }

        /// <inheritdoc />
        public string GetComposeMethodName(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            string methodName = "Singleton" + Type.ToCompileMethodName(simpleName);
            return methodName;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetMethods(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IEnumerable<string> innerMethods = Inner.GetMethods(context, simpleName);
            foreach (string innerMethod in innerMethods)
            {
                yield return innerMethod;
            }

            string methodName = GetComposeMethodName(context, simpleName);
            string signature = $"private {Type.ToCompileName()} {methodName}()";

            string instanceExpression = GetInstanceExpression(context, simpleName);
            instanceExpression = CodeGen.Indent(instanceExpression);

            string method =
                string.Format("{0}{2}{{{2}    return {1};{2}}}", signature, instanceExpression, NewLine);
            yield return method;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetFields(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IEnumerable<string> innerfields = Inner.GetFields(context);
            foreach (string innerfield in innerfields)
            {
                yield return innerfield;
            }

            string field = $"private readonly System.Lazy<{Type.ToCompileName()}> {GetLazyFieldName()};";
            yield return field;
        }

        /// <inheritdoc />
        public IEnumerable<(string snippet, object value)> GetFieldInitializations(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Inner.GetFieldInitializations(context);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAdditionalInitializations(CompositionContext context, bool simpleName)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IEnumerable<string> innerInitializations = Inner.GetAdditionalInitializations(context, simpleName);
            foreach (string innerInitialization in innerInitializations)
            {
                yield return innerInitialization;
            }

            string fieldName = GetLazyFieldName();
            string lazyType = $"System.Lazy<{Type.ToCompileName()}>";

            if (Inner.RequiresConstructionContext(context))
            {
                string contextVar = $"var context = {context.ConstructionContext}.Default;";
                string value = $"return {Inner.GetInstanceExpression(context, simpleName)};";
                string valueFactory = $"() =>{NewLine}{{{NewLine}	{contextVar}{NewLine}	{value}{NewLine}}});";
                valueFactory = CodeGen.Indent(valueFactory);
                string snippet = $"{fieldName} = new {lazyType}({NewLine}    {valueFactory}";
                yield return snippet;
            }
            else
            {
                string valueFactory = $"() => {Inner.GetInstanceExpression(context, simpleName)}";
                string snippet = $"{fieldName} = new {lazyType}({NewLine}    {valueFactory});";
                yield return snippet;
            }
        }

        /// <inheritdoc />
        public bool RequiresConstructionContext(CompositionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return false;
        }

        private string GetLazyFieldName() => "Lazy_" + Type.ToCompileMethodName(simpleName: false);
    }
}
