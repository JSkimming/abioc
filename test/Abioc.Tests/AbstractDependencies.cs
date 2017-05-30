// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.AbstractDependencies;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace AbstractDependencies
    {
        public interface IInterface
        {
        }

        public abstract class AbstractBaseClass
        {
        }

        public class InterfaceImplementation : IInterface
        {
        }

        public class BaseClassImplementation : AbstractBaseClass
        {
        }

        public class BothImplementation : AbstractBaseClass, IInterface
        {
        }

        public class ClassWithInterfaceDependencies
        {
            public ClassWithInterfaceDependencies(IInterface interfaceDependency)
            {
                InterfaceDependency =
                    interfaceDependency ?? throw new ArgumentNullException(nameof(interfaceDependency));
            }

            public IInterface InterfaceDependency { get; }
        }

        public class ClassWithAbstractBaseClassDependencies
        {
            public ClassWithAbstractBaseClassDependencies(AbstractBaseClass abstractBaseClassDependency)
            {
                AbstractBaseClassDependency =
                    abstractBaseClassDependency
                    ?? throw new ArgumentNullException(nameof(abstractBaseClassDependency));
            }

            public AbstractBaseClass AbstractBaseClassDependency { get; }
        }

        public class ClassWithMixedDependencies
        {
            public ClassWithMixedDependencies(
                IInterface interfaceDependency,
                AbstractBaseClass abstractBaseClassDependency,
                InterfaceImplementation interfaceImplementation,
                BaseClassImplementation baseClassImplementation)
            {
                InterfaceDependency =
                    interfaceDependency ?? throw new ArgumentNullException(nameof(interfaceDependency));
                AbstractBaseClassDependency =
                    abstractBaseClassDependency
                    ?? throw new ArgumentNullException(nameof(abstractBaseClassDependency));
                InterfaceImplementation =
                    interfaceImplementation ?? throw new ArgumentNullException(nameof(interfaceImplementation));
                BaseClassImplementation =
                    baseClassImplementation ?? throw new ArgumentNullException(nameof(baseClassImplementation));
            }

            public IInterface InterfaceDependency { get; }
            public AbstractBaseClass AbstractBaseClassDependency { get; }
            public InterfaceImplementation InterfaceImplementation { get; }
            public BaseClassImplementation BaseClassImplementation { get; }
        }
    }

    public abstract class WhenRegisteringAbstractDependenciesBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldCreateAClassWithInterfaceDependencies()
        {
            // Act
            ClassWithInterfaceDependencies actual = GetService<ClassWithInterfaceDependencies>();

            // Assert
            actual.Should().NotBeNull();
            actual.InterfaceDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<InterfaceImplementation>();
        }

        [Fact]
        public void ItShouldCreateAClassWithAbstractBaseClassDependencies()
        {
            // Act
            ClassWithAbstractBaseClassDependencies actual = GetService<ClassWithAbstractBaseClassDependencies>();

            // Assert
            actual.Should().NotBeNull();
            actual.AbstractBaseClassDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<BaseClassImplementation>();
        }

        [Fact]
        public void ItShouldCreateAClassWithMixedDependencies()
        {
            // Act
            ClassWithMixedDependencies actual = GetService<ClassWithMixedDependencies>();

            // Assert
            actual.Should().NotBeNull();
            actual.InterfaceDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<InterfaceImplementation>();
            actual.AbstractBaseClassDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<BaseClassImplementation>();

            actual.InterfaceImplementation
                .Should()
                .NotBeNull()
                .And.BeOfType<InterfaceImplementation>()
                .And.NotBeSameAs(actual.InterfaceDependency);

            actual.BaseClassImplementation
                .Should()
                .NotBeNull()
                .And.BeOfType<BaseClassImplementation>()
                .And.NotBeSameAs(actual.AbstractBaseClassDependency);
        }
    }

    public class WhenRegisteringAbstractDependenciesWithAContext : WhenRegisteringAbstractDependenciesBase
    {
        private readonly IContainer<int> _container;

        public WhenRegisteringAbstractDependenciesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<IInterface, InterfaceImplementation>()
                    .Register<AbstractBaseClass, BaseClassImplementation>()
                    .Register<ClassWithInterfaceDependencies>()
                    .Register<ClassWithAbstractBaseClassDependencies>()
                    .Register<ClassWithMixedDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenRegisteringAbstractDependenciesWithoutAContext : WhenRegisteringAbstractDependenciesBase
    {
        private readonly IContainer _container;

        public WhenRegisteringAbstractDependenciesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<IInterface, InterfaceImplementation>()
                    .Register<AbstractBaseClass, BaseClassImplementation>()
                    .Register<ClassWithInterfaceDependencies>()
                    .Register<ClassWithAbstractBaseClassDependencies>()
                    .Register<ClassWithMixedDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }

    public abstract class WhenRegisteringAClassThatImplementsMultipleAbstractionsBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldCreateAClassWithInterfaceDependencies()
        {
            // Act
            ClassWithInterfaceDependencies actual = GetService<ClassWithInterfaceDependencies>();

            // Assert
            actual.Should().NotBeNull();
            actual.InterfaceDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<BothImplementation>();
        }

        [Fact]
        public void ItShouldCreateAClassWithAbstractBaseClassDependencies()
        {
            // Act
            ClassWithAbstractBaseClassDependencies actual = GetService<ClassWithAbstractBaseClassDependencies>();

            // Assert
            actual.Should().NotBeNull();
            actual.AbstractBaseClassDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<BothImplementation>();
        }

        [Fact]
        public void ItShouldCreateAClassWithMixedDependencies()
        {
            // Act
            ClassWithMixedDependencies actual = GetService<ClassWithMixedDependencies>();

            // Assert
            actual.Should().NotBeNull();
            actual.InterfaceDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<BothImplementation>();
            actual.AbstractBaseClassDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<BothImplementation>();

            actual.InterfaceImplementation
                .Should()
                .NotBeNull()
                .And.BeOfType<InterfaceImplementation>()
                .And.NotBeSameAs(actual.InterfaceDependency);

            actual.BaseClassImplementation
                .Should()
                .NotBeNull()
                .And.BeOfType<BaseClassImplementation>()
                .And.NotBeSameAs(actual.AbstractBaseClassDependency);
        }
    }

    public class WhenRegisteringAClassThatImplementsMultipleAbstractionsWithAContext
        : WhenRegisteringAClassThatImplementsMultipleAbstractionsBase
    {
        private readonly IContainer<int> _container;

        public WhenRegisteringAClassThatImplementsMultipleAbstractionsWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<IInterface, BothImplementation>()
                    .Register<AbstractBaseClass, BothImplementation>()
                    .Register<InterfaceImplementation>()
                    .Register<BaseClassImplementation>()
                    .Register<ClassWithInterfaceDependencies>()
                    .Register<ClassWithAbstractBaseClassDependencies>()
                    .Register<ClassWithMixedDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenRegisteringAClassThatImplementsMultipleAbstractionsWithoutAContext
        : WhenRegisteringAClassThatImplementsMultipleAbstractionsBase
    {
        private readonly IContainer _container;

        public WhenRegisteringAClassThatImplementsMultipleAbstractionsWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<IInterface, BothImplementation>()
                    .Register<AbstractBaseClass, BothImplementation>()
                    .Register<InterfaceImplementation>()
                    .Register<BaseClassImplementation>()
                    .Register<ClassWithInterfaceDependencies>()
                    .Register<ClassWithAbstractBaseClassDependencies>()
                    .Register<ClassWithMixedDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
