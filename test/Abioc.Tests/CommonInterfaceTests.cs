// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class WhenRegisteringTwoClassThatImplementTheSameInterface
    {
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenRegisteringTwoClassThatImplementTheSameInterface()
        {
            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register<ISimpleInterface, SimpleClass1WithoutDependencies>()
                .Register<ISimpleInterface, SimpleClass2WithoutDependencies>()
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldCreateBothImplementationsOfTheSameInterface()
        {
            // Act
            IReadOnlyList<ISimpleInterface> actual = _mappings.GetServices<ISimpleInterface>().ToList();

            // Assert
            actual.Should()
                .HaveCount(2)
                .And.Contain(s => s is SimpleClass1WithoutDependencies)
                .And.Contain(s => s is SimpleClass2WithoutDependencies);
        }

        [Fact]
        public void ItShouldThrowADiExceptionIfGettingAsASingleService()
        {
            // Arrange
            string expectedMessage =
                $"There are multiple registered factories to create services of type '{typeof(ISimpleInterface)}'.";

            // Act
            Action action = () => _mappings.GetService<ISimpleInterface>();

            // Assert
            action
                .ShouldThrow<DiException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenFactoringTwoClassThatImplementTheSameInterface
    {
        private readonly SimpleClass1WithoutDependencies _expected1;
        private readonly SimpleClass2WithoutDependencies _expected2;

        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenFactoringTwoClassThatImplementTheSameInterface()
        {
            _expected1 = new SimpleClass1WithoutDependencies();
            _expected2 = new SimpleClass2WithoutDependencies();

            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register(c => _expected1)
                .Register(c => _expected2)
                .Register<ISimpleInterface, SimpleClass1WithoutDependencies>()
                .Register<ISimpleInterface, SimpleClass2WithoutDependencies>()
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldCreateBothImplementationsOfTheSameInterface()
        {
            // Act
            IReadOnlyList<ISimpleInterface> actual = _mappings.GetServices<ISimpleInterface>().ToList();

            // Assert
            actual.Should()
                .HaveCount(2)
                .And.Contain(_expected1)
                .And.Contain(_expected2);
        }

        [Fact]
        public void ItShouldThrowADiExceptionIfGettingAsASingleService()
        {
            // Arrange
            string expectedMessage =
                $"There are multiple registered factories to create services of type '{typeof(ISimpleInterface)}'.";

            // Act
            Action action = () => _mappings.GetService<ISimpleInterface>();

            // Assert
            action
                .ShouldThrow<DiException>()
                .WithMessage(expectedMessage);
        }
    }
}
