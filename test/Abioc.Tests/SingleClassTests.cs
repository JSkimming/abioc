// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class WhenCreatingASimpleClassWithoutDependencies
    {
        [Fact]
        public void ItShouldCreateTheServiceIfGivenNoFactory()
        {
            // Arrange
            IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mappings =
                new RegistrationContext<DefaultContructionContext>()
                    .Register<SimpleClassWithoutDependencies>()
                    .Compile(GetType().Assembly);

            // Act
            SimpleClassWithoutDependencies actual = mappings.GetService<SimpleClassWithoutDependencies>();

            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateServicesIfGivenNoFactory()
        {
            // Arrange
            IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mappings =
                new RegistrationContext<DefaultContructionContext>()
                    .Register<SimpleClassWithoutDependencies>()
                    .Compile(GetType().Assembly);

            // Act
            IReadOnlyList<SimpleClassWithoutDependencies> actual =
                mappings.GetServices<SimpleClassWithoutDependencies>().ToList();

            actual.Should().HaveCount(1);
            actual[0].Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheServiceUsingAGivenFactory()
        {
            // Arrange
            var expected = new SimpleClassWithoutDependencies();

            IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mappings =
                new RegistrationContext<DefaultContructionContext>()
                    .Register(c => expected)
                    .Compile(GetType().Assembly);

            // Act
            SimpleClassWithoutDependencies actual = mappings.GetService<SimpleClassWithoutDependencies>();

            actual.Should().NotBeNull().And.BeSameAs(expected);
        }

        [Fact]
        public void ItShouldCreateServicesUsingAGivenFactory()
        {
            // Arrange
            var expected = new SimpleClassWithoutDependencies();

            IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> mappings =
                new RegistrationContext<DefaultContructionContext>()
                    .Register(c => expected)
                    .Compile(GetType().Assembly);

            // Act
            SimpleClassWithoutDependencies actual = mappings.GetService<SimpleClassWithoutDependencies>();

            actual.Should().NotBeNull().And.BeSameAs(expected);
        }
    }
}
