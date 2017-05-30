// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class WhenCreatingASimpleClassWithoutDependenciesBase
    {
        protected abstract TService GetService<TService>();

        protected abstract IEnumerable<TService> GetServices<TService>();

        [Fact]
        public void ItShouldCreateTheService()
        {
            // Act
            SimpleClass1WithoutDependencies actual = GetService<SimpleClass1WithoutDependencies>();

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateServices()
        {
            // Act
            IReadOnlyList<SimpleClass1WithoutDependencies> actual =
                GetServices<SimpleClass1WithoutDependencies>().ToList();

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
            Action action = () => GetService<SimpleClass2WithoutDependencies>();

            // Assert
            action
                .ShouldThrow<DiException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public void ItShouldReturnAnEmptyListIfGettingUnregisteredServices()
        {
            // Act
            IEnumerable<SimpleClass2WithoutDependencies> actual =
                GetServices<SimpleClass2WithoutDependencies>();

            // Assert
            // Assert
            actual.Should().BeEmpty();
        }
    }

    public class WhenCreatingASimpleClassWithoutDependenciesWithAContext
        : WhenCreatingASimpleClassWithoutDependenciesBase
    {
        private readonly IContainer<int> _container;

        public WhenCreatingASimpleClassWithoutDependenciesWithAContext()
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<SimpleClass1WithoutDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);

        protected override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>(1);
    }

    public class WhenCreatingASimpleClassWithoutDependenciesWithoutAContext
        : WhenCreatingASimpleClassWithoutDependenciesBase
    {
        private readonly IContainer _container;

        public WhenCreatingASimpleClassWithoutDependenciesWithoutAContext()
        {
            _container =
                new RegistrationSetup()
                    .Register<SimpleClass1WithoutDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();

        protected override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>();
    }

    public abstract class WhenFactoringASimpleClassWithoutDependenciesBase
    {
        protected SimpleClass1WithoutDependencies Expected;

        protected abstract TService GetService<TService>();

        protected abstract IEnumerable<TService> GetServices<TService>();

        [Fact]
        public void ItShouldCreateTheServiceUsingTheGivenFactory()
        {
            // Act
            SimpleClass1WithoutDependencies actual = GetService<SimpleClass1WithoutDependencies>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(Expected);
        }

        [Fact]
        public void ItShouldCreateServicesUsingTheGivenFactory()
        {
            // Act
            IReadOnlyList<SimpleClass1WithoutDependencies> actual =
                GetServices<SimpleClass1WithoutDependencies>().ToList();

            // Assert
            actual.Should().HaveCount(1);
            actual[0].Should().BeSameAs(Expected);
        }

        [Fact]
        public void ItShouldThrowADiExceptionIfGettingAnUnregisteredService()
        {
            // Arrange
            string expectedMessage =
                "There is no registered factory to create services of type" +
                $" '{typeof(SimpleClass2WithoutDependencies)}'.";

            // Act
            Action action = () => GetService<SimpleClass2WithoutDependencies>();

            // Assert
            action
                .ShouldThrow<DiException>()
                .WithMessage(expectedMessage);
        }

        [Fact]
        public void ItShouldReturnAnEmptyListIfGettingUnregisteredServices()
        {
            // Act
            IEnumerable<SimpleClass2WithoutDependencies> actual =
                GetServices<SimpleClass2WithoutDependencies>();

            // Assert
            actual.Should().BeEmpty();
        }
    }

    public class WhenFactoringASimpleClassWithoutDependenciesWithAContext
        : WhenFactoringASimpleClassWithoutDependenciesBase
    {
        private readonly IContainer<int> _container;

        public WhenFactoringASimpleClassWithoutDependenciesWithAContext(ITestOutputHelper output)
        {
            // Arrange
            Expected = new SimpleClass1WithoutDependencies();

            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory(c => Expected)
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);

        protected override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>(1);
    }

    public class WhenFactoringASimpleClassWithoutDependenciesWithoutAContext
        : WhenFactoringASimpleClassWithoutDependenciesBase
    {
        private readonly IContainer _container;

        public WhenFactoringASimpleClassWithoutDependenciesWithoutAContext(ITestOutputHelper output)
        {
            // Arrange
            Expected = new SimpleClass1WithoutDependencies();

            _container =
                new RegistrationSetup()
                    .RegisterFactory(() => Expected)
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();

        protected override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>();
    }
}
