// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Composition;
    using Abioc.PropertyInjectionTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace PropertyInjectionTests
    {
        public interface IInterfaceDependency1
        {
        }

        public interface IInterfaceDependency2
        {
        }

        public interface IInterfaceDependency3
        {
        }

        public class ConcreteDependency1 : IInterfaceDependency1
        {
        }

        public class ConcreteDependency2 : IInterfaceDependency2
        {
        }

        public class ConcreteDependency3 : IInterfaceDependency3
        {
        }

        public class ConcreteClassWith1InjectedProperty
        {
            public ConcreteClassWith1InjectedProperty(
                IInterfaceDependency1 dependency1,
                IInterfaceDependency2 dependency2)
            {
                Dependency1 = dependency1 ?? throw new ArgumentNullException(nameof(dependency1));
                Dependency2 = dependency2 ?? throw new ArgumentNullException(nameof(dependency2));
            }

            public IInterfaceDependency1 Dependency1 { get; }
            public IInterfaceDependency2 Dependency2 { get; }
            public IInterfaceDependency3 Dependency3 { get; set; }
        }

        public class ConcreteClassWith2InjectedProperties
        {
            public ConcreteClassWith2InjectedProperties(
                IInterfaceDependency1 dependency1)
            {
                Dependency1 = dependency1 ?? throw new ArgumentNullException(nameof(dependency1));
            }

            public IInterfaceDependency1 Dependency1 { get; }
            public IInterfaceDependency2 Dependency2 { get; set; }
            public IInterfaceDependency3 Dependency3 { get; set; }
        }

        public class ConcreteClassWith3InjectedProperties
        {
            public IInterfaceDependency1 Dependency1 { get; set; }
            public IInterfaceDependency2 Dependency2 { get; set; }
            public IInterfaceDependency3 Dependency3 { get; set; }
        }

        public class ConcreteClassWithSetOnlyProperty
        {
            private IInterfaceDependency1 _dependency1;
            private IInterfaceDependency2 _dependency2;
            private IInterfaceDependency3 _dependency3;

            public IInterfaceDependency1 Dependency1 { set => _dependency1 = value; }
            public IInterfaceDependency2 Dependency2 { set => _dependency2 = value; }
            public IInterfaceDependency3 Dependency3 { set => _dependency3 = value; }

            public IInterfaceDependency1 GetDependency1() => _dependency1;
            public IInterfaceDependency2 GetDependency2() => _dependency2;
            public IInterfaceDependency3 GetDependency3() => _dependency3;
        }

        public class ConcreteClassWithMixedDependencyInjection
        {
            private IInterfaceDependency3 _dependency3;

            public ConcreteClassWithMixedDependencyInjection(
                IInterfaceDependency1 dependency1)
            {
                Dependency1 = dependency1 ?? throw new ArgumentNullException(nameof(dependency1));
            }

            public IInterfaceDependency1 Dependency1 { get; }
            public IInterfaceDependency2 Dependency2 { get; set; }
            public IInterfaceDependency3 Dependency3 { set => _dependency3 = value; }

            public IInterfaceDependency3 GetDependency3() => _dependency3;
        }

        public class ClassWithEnumerablePropertyDependency
        {
            public IEnumerable<ConcreteDependency1> Dependency { get; set; }
        }

        public interface IUnsupportedGenericInterface<T>
        {
        }

        public class ClassWithUnsupportedGenericPropertyDependency
        {
            public IUnsupportedGenericInterface<ConcreteDependency1> Dependency { get; set; }
        }
    }

    public abstract class WhenRegisteringClassesWithInjectedPropertiesBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldCreateAClassWith1InjectedProperty()
        {
            // Act
            ConcreteClassWith1InjectedProperty actual = GetService<ConcreteClassWith1InjectedProperty>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(GetService<ConcreteClassWith1InjectedProperty>());
            actual.Dependency1
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency1>();
            actual.Dependency2
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency2>();
            actual.Dependency3
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency3>();
        }

        [Fact]
        public void ItShouldCreateAClassWith2InjectedProperties()
        {
            // Act
            ConcreteClassWith2InjectedProperties actual = GetService<ConcreteClassWith2InjectedProperties>();

            // Assert
            ConcreteClassWith2InjectedProperties other = GetService<ConcreteClassWith2InjectedProperties>();
            actual
                .Should()
                .NotBeNull()
                .And.NotBeSameAs(other);
            actual.Dependency1
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency1>()
                .And.BeSameAs(other.Dependency1);
            actual.Dependency2
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency2>();
            actual.Dependency3
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency3>()
                .And.BeSameAs(other.Dependency3);
        }

        [Fact]
        public void ItShouldCreateAClassWith3InjectedProperties()
        {
            // Act
            ConcreteClassWith3InjectedProperties actual = GetService<ConcreteClassWith3InjectedProperties>();

            // Assert
            ConcreteClassWith3InjectedProperties other = GetService<ConcreteClassWith3InjectedProperties>();
            actual
                .Should()
                .NotBeNull()
                .And.NotBeSameAs(other);
            actual.Dependency1
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency1>()
                .And.BeSameAs(other.Dependency1);
            actual.Dependency2
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency2>();
            actual.Dependency3
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency3>()
                .And.BeSameAs(other.Dependency3);
        }

        [Fact]
        public void ItShouldCreateAClassWithSetOnlyProperties()
        {
            // Act
            ConcreteClassWithSetOnlyProperty actual = GetService<ConcreteClassWithSetOnlyProperty>();

            // Assert
            ConcreteClassWithSetOnlyProperty other = GetService<ConcreteClassWithSetOnlyProperty>();
            actual
                .Should()
                .NotBeNull()
                .And.NotBeSameAs(other);
            actual.GetDependency1()
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency1>()
                .And.BeSameAs(other.GetDependency1());
            actual.GetDependency2()
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency2>();
            actual.GetDependency3()
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency3>()
                .And.BeSameAs(other.GetDependency3());
        }

        [Fact]
        public void ItShouldCreateAConcreteClassWithMixedDependencyInjection()
        {
            // Act
            ConcreteClassWithMixedDependencyInjection actual = GetService<ConcreteClassWithMixedDependencyInjection>();

            // Assert
            ConcreteClassWithMixedDependencyInjection other = GetService<ConcreteClassWithMixedDependencyInjection>();
            actual
                .Should()
                .NotBeNull()
                .And.NotBeSameAs(other);
            actual.Dependency1
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency1>()
                .And.BeSameAs(other.Dependency1);
            actual.Dependency2
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency2>();
            actual.GetDependency3()
                .Should()
                .NotBeNull()
                .And.BeOfType<ConcreteDependency3>()
                .And.BeSameAs(other.GetDependency3());
        }
    }

    public class WhenRegisteringClassesWithInjectedPropertiesWithAContext
        : WhenRegisteringClassesWithInjectedPropertiesBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringClassesWithInjectedPropertiesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    // Dependencies
                    .RegisterSingleton<IInterfaceDependency1, ConcreteDependency1>()
                    .Register<IInterfaceDependency2, ConcreteDependency2>()
                    .RegisterFixed<IInterfaceDependency3>(new ConcreteDependency3())
                    // Concrete classes
                    .RegisterSingleton<ConcreteClassWith1InjectedProperty>(
                        composer => composer
                            .InjectProperty(s => s.Dependency3))
                    .Register<ConcreteClassWith2InjectedProperties>(
                        composer => composer
                            .InjectProperty(s => s.Dependency2)
                            .InjectProperty(s => s.Dependency3))
                    .RegisterFactory(context => new ConcreteClassWith3InjectedProperties(),
                        composer => composer
                            .InjectProperty(s => s.Dependency1)
                            .InjectProperty(s => s.Dependency2)
                            .InjectProperty(s => s.Dependency3))
                    .Register(typeof(ConcreteClassWithSetOnlyProperty),
                        composer => composer.InjectAllProperties())
                    .Register<ConcreteClassWithMixedDependencyInjection>(
                        composer => composer.InjectAllProperties())
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenRegisteringClassesWithInjectedPropertiesWithoutAContext
        : WhenRegisteringClassesWithInjectedPropertiesBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringClassesWithInjectedPropertiesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    // Dependencies
                    .RegisterSingleton<IInterfaceDependency1, ConcreteDependency1>()
                    .Register<IInterfaceDependency2, ConcreteDependency2>()
                    .RegisterFixed<IInterfaceDependency3>(new ConcreteDependency3())
                    // Concrete classes
                    .RegisterSingleton<ConcreteClassWith1InjectedProperty>(
                        composer => composer
                            .InjectProperty(s => s.Dependency3))
                    .Register<ConcreteClassWith2InjectedProperties>(
                        composer => composer
                            .InjectProperty(s => s.Dependency2)
                            .InjectProperty(s => s.Dependency3))
                    .RegisterFactory(() => new ConcreteClassWith3InjectedProperties(),
                        composer => composer
                            .InjectProperty(s => s.Dependency1)
                            .InjectProperty(s => s.Dependency2)
                            .InjectProperty(s => s.Dependency3))
                    .Register(typeof(ConcreteClassWithSetOnlyProperty),
                        composer => composer.InjectAllProperties())
                    .Register<ConcreteClassWithMixedDependencyInjection>(
                        composer => composer.InjectAllProperties())
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }

    public class WhenRegistrationIsMissingPropertyInjectedDependencies
    {
        private readonly RegistrationSetup _setup;

        public WhenRegistrationIsMissingPropertyInjectedDependencies()
        {
            _setup =
                new RegistrationSetup()
                    // Dependencies
                    .Register<IInterfaceDependency1, ConcreteDependency1>()
                    .Register<IInterfaceDependency3, ConcreteDependency3>()
                    // Concrete classes
                    .Register<ConcreteClassWithSetOnlyProperty>(
                        composer => composer.InjectAllProperties());
        }

        [Fact]
        public void ItShouldThowACompositionException()
        {
            // Arrange
            string expectedMessage =
                "Failed to get the composition for the property 'IInterfaceDependency2 Dependency2' of the instance " +
                $"'{typeof(ConcreteClassWithSetOnlyProperty)}'. Is there a missing registration mapping?";

            // Act
            Action action = () => _setup.Compose().GenerateCode(_setup.Registrations);

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public abstract class WhenAddingAnInjectAllPropertiesAfterHavingAddedAnInjectPropertyBase
    {
        protected Action Action;

        [Fact]
        public void ItShouldThowARegistrationException()
        {
            // Arrange
            string expectedMessage =
                $"Cannot inject all properties of '{typeof(ConcreteClassWith3InjectedProperties)}' if existing " +
                "property injection dependencies have already been registered.";

            // Act/Assert
            Action
                .ShouldThrow<RegistrationException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenAddingAnInjectAllPropertiesAfterHavingAddedAnInjectPropertyWithAContext
        : WhenAddingAnInjectAllPropertiesAfterHavingAddedAnInjectPropertyBase
    {
        public WhenAddingAnInjectAllPropertiesAfterHavingAddedAnInjectPropertyWithAContext()
        {
            Action = () => new RegistrationSetup<int>()
                .Register<ConcreteClassWith3InjectedProperties>(
                    composer => composer
                        .InjectProperty(s => s.Dependency1)
                        .InjectAllProperties());
        }
    }

    public class WhenAddingAnInjectAllPropertiesAfterHavingAddedAnInjectPropertyWithoutAContext
        : WhenAddingAnInjectAllPropertiesAfterHavingAddedAnInjectPropertyBase
    {
        public WhenAddingAnInjectAllPropertiesAfterHavingAddedAnInjectPropertyWithoutAContext()
        {
            Action = () => new RegistrationSetup()
                .Register<ConcreteClassWith3InjectedProperties>(
                    composer => composer
                        .InjectProperty(s => s.Dependency1)
                        .InjectAllProperties());
        }
    }

    public abstract class WhenAddingAnInjectPropertyAfterHavingAddedAnInjectAllPropertiesBase
    {
        protected Action Action;

        [Fact]
        public void ItShouldThowARegistrationException()
        {
            // Arrange
            string expectedMessage =
                "Cannot add the property 's => s.Dependency1' as it has already been specified that all properties " +
                $"of '{typeof(ConcreteClassWith3InjectedProperties)}' need to be injected as a dependency.";

            // Act/Assert
            Action
                .ShouldThrow<RegistrationException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenAddingAnInjectPropertyAfterHavingAddedAnInjectAllPropertiesWithAContext
        : WhenAddingAnInjectPropertyAfterHavingAddedAnInjectAllPropertiesBase
    {
        public WhenAddingAnInjectPropertyAfterHavingAddedAnInjectAllPropertiesWithAContext()
        {
            Action = () => new RegistrationSetup<int>()
                .Register<ConcreteClassWith3InjectedProperties>(
                    composer => composer
                        .InjectAllProperties()
                        .InjectProperty(s => s.Dependency1));
        }
    }

    public class WhenAddingAnInjectPropertyAfterHavingAddedAnInjectAllPropertiesWithoutAContext
        : WhenAddingAnInjectPropertyAfterHavingAddedAnInjectAllPropertiesBase
    {
        public WhenAddingAnInjectPropertyAfterHavingAddedAnInjectAllPropertiesWithoutAContext()
        {
            Action = () => new RegistrationSetup()
                .Register<ConcreteClassWith3InjectedProperties>(
                    composer => composer
                        .InjectAllProperties()
                        .InjectProperty(s => s.Dependency1));
        }
    }

    public class WhenRegisteringClassWithAnEnumerablePropertyDependency
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringClassWithAnEnumerablePropertyDependency(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<ConcreteDependency1>()
                    .Register<ClassWithEnumerablePropertyDependency>(
                        composer => composer.InjectAllProperties())
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        [Fact]
        public void ItShouldCreateAClassWithAnInjectedProperty()
        {
            // Act
            ClassWithEnumerablePropertyDependency actual =
                _container.GetService<ClassWithEnumerablePropertyDependency>();

            // Assert
            actual
                .Should()
                .NotBeNull();
            actual.Dependency
                .Should()
                .NotBeNullOrEmpty();
        }
    }

    public class WhenRegisteringClassWithAnUnsupportedGenericPropertyDependency
    {
        private readonly RegistrationSetup _setup;

        public WhenRegisteringClassWithAnUnsupportedGenericPropertyDependency()
        {
            _setup =
                new RegistrationSetup()
                    .Register<ConcreteDependency1>()
                    .Register<ClassWithUnsupportedGenericPropertyDependency>(
                        composer => composer.InjectAllProperties());
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"Failed to get the composition for the property '{typeof(IUnsupportedGenericInterface<>).Name} " +
                $"Dependency' of the instance '{typeof(ClassWithUnsupportedGenericPropertyDependency)}'. " +
                "Is there a missing registration mapping?";

            // Act
            Action action = () => _setup.Compose().GenerateCode(_setup.Registrations);

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }
}
