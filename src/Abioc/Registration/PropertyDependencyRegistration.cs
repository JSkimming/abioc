// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// A <see cref="IRegistration"/> entry that produces the code to use a injected constant value.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PropertyDependencyRegistration : RegistrationBase
    {
        private readonly List<LambdaExpression> _propertyExpressions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDependencyRegistration"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="Inner"/> <see cref="IRegistration"/>.</param>
        public PropertyDependencyRegistration(IRegistration inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            Inner = inner;
            InjectAllProperties = true;
            _propertyExpressions = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDependencyRegistration"/> class.
        /// </summary>
        /// <param name="inner">The <see cref="Inner"/> <see cref="IRegistration"/>.</param>
        /// <param name="property">
        /// An expression used to specify a property of the <see cref="ImplementationType"/> that needs to be injected
        /// as a dependency.
        /// </param>
        public PropertyDependencyRegistration(IRegistration inner, LambdaExpression property)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            Inner = inner;
            InjectAllProperties = false;
            _propertyExpressions =
                new List<LambdaExpression>(1)
                {
                    property,
                };
        }

        /// <summary>
        /// Gets the <see cref="IRegistration.ImplementationType"/> of the <see cref="IRegistration"/>.
        /// </summary>
        public override Type ImplementationType => Inner.ImplementationType;

        /// <summary>
        /// Gets the <see cref="Inner"/> <see cref="IRegistration"/>.
        /// </summary>
        public IRegistration Inner { get; }

        /// <summary>
        /// Gets a value indicating whether all the properties of the <see cref="ImplementationType"/> need to be
        /// injected as a dependency.
        /// </summary>
        public bool InjectAllProperties { get; }

        /// <summary>
        /// Gets the list of expressions specifying the properties of the <see cref="ImplementationType"/> that need to
        /// be injected as a dependency.
        /// </summary>
        public IReadOnlyList<LambdaExpression> PropertyExpressions => _propertyExpressions;

        private string DebuggerDisplay => $"{GetType().Name}: Type={ImplementationType.Name}";

        /// <summary>
        /// Adds the <paramref name="property"/> to the <see cref="PropertyExpressions"/>.
        /// </summary>
        /// <param name="property">
        /// An expression used to specify a property of the <see cref="ImplementationType"/> that needs to be injected
        /// as a dependency.
        /// </param>
        public void AddInjectedProperty(LambdaExpression property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (InjectAllProperties)
            {
                string message =
                    $"Cannot add the property '{property}' as it has already been specified that all properties of " +
                    $"'{ImplementationType}' need to be injected as a dependency.";
                throw new RegistrationException(message);
            }

            _propertyExpressions.Add(property);
        }
    }
}
