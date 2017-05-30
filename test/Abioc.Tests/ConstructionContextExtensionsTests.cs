// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using Xunit;

    public class WhenUpdatingAConstructionContext
    {
        private readonly ConstructionContext<string> _initialValue;

        public WhenUpdatingAConstructionContext()
        {
            string extra = Guid.NewGuid().ToString("D").ToUpperInvariant();
            IContainer<string> container = new Mock<IContainer<string>>().Object;
            _initialValue = ConstructionContext<string>.Default.Initialize(container, extra);
        }

        [Fact]
        public void ItShouldUpdateImplementationTypeIfSpecified()
        {
            // Act
            ConstructionContext<string> context = _initialValue.Update(implementationType: GetType());

            // Assert
            context.ImplementationType.Should().Be(GetType());
            context.ServiceType.Should().Be(typeof(void));
            context.RecipientType.Should().Be(typeof(void));
            context.Container.Should().Be(_initialValue.Container);
            context.Extra.Should().Be(_initialValue.Extra);
        }

        [Fact]
        public void ItShouldUpdateServiceTypeIfSpecified()
        {
            // Act
            ConstructionContext<string> context = _initialValue.Update(serviceType: GetType());

            // Assert
            context.ImplementationType.Should().Be(typeof(void));
            context.ServiceType.Should().Be(GetType());
            context.RecipientType.Should().Be(typeof(void));
            context.Container.Should().Be(_initialValue.Container);
            context.Extra.Should().Be(_initialValue.Extra);
        }

        [Fact]
        public void ItShouldUpdateRecipientTypeIfSpecified()
        {
            // Act
            ConstructionContext<string> context = _initialValue.Update(recipientType: GetType());

            // Assert
            context.ImplementationType.Should().Be(typeof(void));
            context.ServiceType.Should().Be(typeof(void));
            context.RecipientType.Should().Be(GetType());
            context.Container.Should().Be(_initialValue.Container);
            context.Extra.Should().Be(_initialValue.Extra);
        }
    }
}
