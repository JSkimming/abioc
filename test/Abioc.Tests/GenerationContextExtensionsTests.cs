// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Generation;
    using Abioc.GenerationContextExtensionsTests;
    using FluentAssertions;
    using Xunit;

    namespace GenerationContextExtensionsTests
    {
        using Abioc.Composition;

        internal class TestGenerationContext : IGenerationContext
        {
            public IReadOnlyDictionary<Type, IComposition> Compositions { get; set; }

            public string ConstructionContext { get; set; }

            public bool HasConstructionContext { get; set; }

            public bool UsingSimpleNames { get; set; }

            public ConstructionContextDefinition ConstructionContextDefinition { get; set; }

            public IGenerationContext Customize(ConstructionContextDefinition constructionContextDefinition)
            {
                return new TestGenerationContext { ConstructionContextDefinition = constructionContextDefinition };
            }
        }
    }

    public class WhenCustomizingAGenerationContext
    {
        private readonly IGenerationContext _initialValue;

        public WhenCustomizingAGenerationContext()
        {
            _initialValue = new TestGenerationContext()
            {
                ConstructionContextDefinition =
                    new ConstructionContextDefinition(typeof(void), typeof(void), typeof(void)),
            };
        }

        [Fact]
        public void ItShouldUpdateImplementationTypeIfSpecified()
        {
            // Act
            IGenerationContext context = _initialValue.Customize(implementationType: GetType());

            // Assert
            context.ConstructionContextDefinition.ImplementationType.Should().Be(GetType());
            context.ConstructionContextDefinition.ServiceType.Should().Be(typeof(void));
            context.ConstructionContextDefinition.RecipientType.Should().Be(typeof(void));
        }

        [Fact]
        public void ItShouldUpdateServiceTypeIfSpecified()
        {
            // Act
            IGenerationContext context = _initialValue.Customize(serviceType: GetType());

            // Assert
            context.ConstructionContextDefinition.ImplementationType.Should().Be(typeof(void));
            context.ConstructionContextDefinition.ServiceType.Should().Be(GetType());
            context.ConstructionContextDefinition.RecipientType.Should().Be(typeof(void));
        }

        [Fact]
        public void ItShouldUpdateRecipientTypeIfSpecified()
        {
            // Act
            IGenerationContext context = _initialValue.Customize(recipientType: GetType());

            // Assert
            context.ConstructionContextDefinition.ImplementationType.Should().Be(typeof(void));
            context.ConstructionContextDefinition.ServiceType.Should().Be(typeof(void));
            context.ConstructionContextDefinition.RecipientType.Should().Be(GetType());
        }
    }

    public class WhenGettingUpdateParameterExpressionsOfAGenerationContext
    {
        private readonly IGenerationContext _initialValue;

        public WhenGettingUpdateParameterExpressionsOfAGenerationContext()
        {
            _initialValue = new TestGenerationContext()
            {
                ConstructionContextDefinition =
                    new ConstructionContextDefinition(typeof(void), typeof(void), typeof(void)),
            };
        }

        [Fact]
        public void ItShouldReturnAnExpressionGotTheImplementationType()
        {
            // Arrange
            string expected = $"implementationType: typeof({GetType()})";

            // Act
            IEnumerable<string> expressions =
                _initialValue.GetUpdateParameterExpressions(implementationType: GetType());

            // Assert
            expressions.ToList().Should().OnlyContain(e => e == expected);
        }

        [Fact]
        public void ItShouldReturnAnExpressionGotTheServiceType()
        {
            // Arrange
            string expected = $"serviceType: typeof({GetType()})";

            // Act
            IEnumerable<string> expressions =
                _initialValue.GetUpdateParameterExpressions(serviceType: GetType());

            // Assert
            expressions.ToList().Should().OnlyContain(e => e == expected);
        }

        [Fact]
        public void ItShouldReturnAnExpressionGotTheRecipientType()
        {
            // Arrange
            string expected = $"recipientType: typeof({GetType()})";

            // Act
            IEnumerable<string> expressions =
                _initialValue.GetUpdateParameterExpressions(recipientType: GetType());

            // Assert
            expressions.ToList().Should().OnlyContain(e => e == expected);
        }
    }
}
