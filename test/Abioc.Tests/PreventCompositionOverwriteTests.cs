// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Composition;
    using Abioc.PreventCompositionOverwriteTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace PreventCompositionOverwriteTests
    {
        public interface IInterface1
        {
        }

        public interface IInterface2
        {
        }

        public class ConcreteClassImplementing2Interfaces : IInterface1, IInterface2
        {
        }

        public class ConcreteDerivedClass : ConcreteClassImplementing2Interfaces
        {
        }
    }

    public class WhenAMappingFrom2InterfacesToAnImplementationIsRegisteredAsWellAsAFactory
    {
        private readonly AbiocContainer _container;

        public WhenAMappingFrom2InterfacesToAnImplementationIsRegisteredAsWellAsAFactory(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<IInterface1, ConcreteClassImplementing2Interfaces>()
                    .RegisterFactory<ConcreteClassImplementing2Interfaces>(() => new ConcreteDerivedClass())
                    .Register<IInterface2, ConcreteClassImplementing2Interfaces>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        [Fact]
        public void ItShouldResolveInterface1()
        {
            // Act
            IInterface1 actual = _container.GetService<IInterface1>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDerivedClass>();
        }

        [Fact]
        public void ItShouldResolveInterface2()
        {
            // Act
            IInterface2 actual = _container.GetService<IInterface2>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDerivedClass>();
        }

        [Fact]
        public void ItShouldResolveTheConcreteClass()
        {
            // Act
            ConcreteClassImplementing2Interfaces actual =
                _container.GetService<ConcreteClassImplementing2Interfaces>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDerivedClass>();
        }

        [Fact]
        public void ItShouldNotResolvedTheConcreteDerivedClass()
        {
            // Act
            IEnumerable<ConcreteDerivedClass> actual =
                _container.GetServices<ConcreteDerivedClass>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }
    }

    public class WhenAMappingFromAnInterfaceToAnImplementationIsRegisteredAsWellAsASingleton
    {
        private readonly AbiocContainer _container;

        public WhenAMappingFromAnInterfaceToAnImplementationIsRegisteredAsWellAsASingleton(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .RegisterSingleton<IInterface2, ConcreteClassImplementing2Interfaces>()
                    .Register<IInterface1, ConcreteClassImplementing2Interfaces>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        [Fact]
        public void ItShouldResolveInterface1()
        {
            // Act
            IInterface1 actual = _container.GetService<IInterface1>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteClassImplementing2Interfaces>();
        }

        [Fact]
        public void ItShouldResolveInterface2()
        {
            // Act
            IInterface2 actual = _container.GetService<IInterface2>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteClassImplementing2Interfaces>();
        }

        [Fact]
        public void ItShouldNotResolveTheConcreteClass()
        {
            // Act
            IEnumerable<ConcreteClassImplementing2Interfaces> actual =
                _container.GetServices<ConcreteClassImplementing2Interfaces>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void TheInterfacesShouldResolveToTheSameInstances()
        {
            // Act
            IInterface1 actual = _container.GetService<IInterface1>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(_container.GetService<IInterface2>());
        }
    }

    public class WhenAMappingFromAnInterfaceToAnImplementationIsRegisteredAsWellAsAnInjectedSingleton
    {
        private readonly AbiocContainer _container;

        public WhenAMappingFromAnInterfaceToAnImplementationIsRegisteredAsWellAsAnInjectedSingleton(
            ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<IInterface1, ConcreteClassImplementing2Interfaces>()
                    .RegisterFixed<IInterface2, ConcreteClassImplementing2Interfaces>(new ConcreteDerivedClass())
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        [Fact]
        public void ItShouldResolveInterface1()
        {
            // Act
            IInterface1 actual = _container.GetService<IInterface1>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDerivedClass>();
        }

        [Fact]
        public void ItShouldResolveInterface2()
        {
            // Act
            IInterface2 actual = _container.GetService<IInterface2>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDerivedClass>();
        }

        [Fact]
        public void ItShouldNotResolveTheConcreteClass()
        {
            // Act
            IEnumerable<ConcreteClassImplementing2Interfaces> actual =
                _container.GetServices<ConcreteClassImplementing2Interfaces>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void TheInterfacesShouldResolveToTheSameInstances()
        {
            // Act
            IInterface1 actual = _container.GetService<IInterface1>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(_container.GetService<IInterface2>());
        }
    }

    public class WhenAFactoryAndASingleAreRegisteredForTheSameImplementation
    {
        private readonly RegistrationSetup _setup;

        public WhenAFactoryAndASingleAreRegisteredForTheSameImplementation()
        {
            _setup =
                new RegistrationSetup()
                    .RegisterSingleton<IInterface1, ConcreteClassImplementing2Interfaces>()
                    .RegisterFactory<IInterface2, ConcreteClassImplementing2Interfaces>(
                        () => new ConcreteClassImplementing2Interfaces());
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"There is already a composition for '{typeof(ConcreteClassImplementing2Interfaces)}', are there " +
                "multiple registrations.*";

            // Act
            Action action = () => _setup.Compose();

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenAFactoryAndAnInjectedSingleAreRegisteredForTheSameImplementation
    {
        private readonly RegistrationSetup _setup;

        public WhenAFactoryAndAnInjectedSingleAreRegisteredForTheSameImplementation()
        {
            _setup =
                new RegistrationSetup()
                    .RegisterFactory<IInterface2, ConcreteClassImplementing2Interfaces>(
                        () => new ConcreteClassImplementing2Interfaces())
                    .RegisterFixed<IInterface1, ConcreteClassImplementing2Interfaces>(
                        new ConcreteClassImplementing2Interfaces());
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"There is already a composition for '{typeof(ConcreteClassImplementing2Interfaces)}', are there " +
                "multiple registrations.*";

            // Act
            Action action = () => _setup.Compose();

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenASingletonAndAnInjectedSingleAreRegisteredForTheSameImplementation
    {
        private readonly RegistrationSetup _setup;

        public WhenASingletonAndAnInjectedSingleAreRegisteredForTheSameImplementation()
        {
            _setup =
                new RegistrationSetup()
                    .RegisterSingleton<IInterface2, ConcreteClassImplementing2Interfaces>()
                    .RegisterFixed<IInterface1, ConcreteClassImplementing2Interfaces>(
                        new ConcreteClassImplementing2Interfaces());
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"There is already a composition for '{typeof(ConcreteClassImplementing2Interfaces)}', are there " +
                "multiple registrations.*";

            // Act
            Action action = () => _setup.Compose();

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenRemovingAMissingComposition
    {
        private readonly CompositionContainer _composition;

        public WhenRemovingAMissingComposition()
        {
            _composition =
                new RegistrationSetup()
                    .Register<IInterface2, ConcreteClassImplementing2Interfaces>()
                    .Compose();
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"There is no current composition for the type '{GetType()}'.";

            // Act
            Action action = () => _composition.RemoveComposition(GetType());

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }
}
