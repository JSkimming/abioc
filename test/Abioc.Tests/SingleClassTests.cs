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
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenCreatingASimpleClassWithoutDependencies()
        {
            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register<SimpleClass1WithoutDependencies>()
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldCreateTheService()
        {
            // Act
            SimpleClass1WithoutDependencies actual = _mappings.GetService<SimpleClass1WithoutDependencies>();

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateServices()
        {
            // Act
            IReadOnlyList<SimpleClass1WithoutDependencies> actual =
                _mappings.GetServices<SimpleClass1WithoutDependencies>().ToList();

            // Assert
            actual.Should().HaveCount(1);
            actual[0].Should().NotBeNull();
        }

        [Fact]
        public void ItShouldThrowADiExceptionIfGettingAnUnregisteredService()
        {
            // Arrange
            string expectedMessage =
                "There is no registered factory to create services of type" +
                $" '{typeof(SimpleClass2WithoutDependencies)}'.";

            // Act
            Action action = () => _mappings.GetService<SimpleClass2WithoutDependencies>();

            // Assert
            action
                .ShouldThrow<DiExcception>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenFactoringASimpleClassWithoutDependencies
    {
        private readonly SimpleClass1WithoutDependencies _expected;

        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenFactoringASimpleClassWithoutDependencies()
        {
            // Arrange
            _expected = new SimpleClass1WithoutDependencies();

            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register(c => _expected)
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldCreateTheServiceUsingTheGivenFactory()
        {
            // Act
            SimpleClass1WithoutDependencies actual = _mappings.GetService<SimpleClass1WithoutDependencies>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(_expected);
        }

        [Fact]
        public void ItShouldCreateServicesUsingTheGivenFactory()
        {
            // Act
            IReadOnlyList<SimpleClass1WithoutDependencies> actual =
                _mappings.GetServices<SimpleClass1WithoutDependencies>().ToList();

            // Assert
            actual.Should().HaveCount(1);
            actual[0].Should().BeSameAs(_expected);
        }

        [Fact]
        public void ItShouldThrowADiExceptionIfGettingAnUnregisteredService()
        {
            // Arrange
            string expectedMessage =
                "There is no registered factory to create services of type" +
                $" '{typeof(SimpleClass2WithoutDependencies)}'.";

            // Act
            Action action = () => _mappings.GetService<SimpleClass2WithoutDependencies>();

            // Assert
            action
                .ShouldThrow<DiExcception>()
                .WithMessage(expectedMessage);
        }
    }
}
