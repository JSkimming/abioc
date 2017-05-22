// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Composition;
    using Abioc.EnumerableDependencyTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace EnumerableDependencyTests
    {
        public interface IMultipleDependency
        {
        }

        public interface IZeroDependency
        {
        }

        public interface ISingleDependency
        {
        }

        public class MultipleDependency1 : IMultipleDependency
        {
        }

        public class MultipleDependency2 : IMultipleDependency
        {
        }

        public class MultipleDependency3 : IMultipleDependency
        {
        }

        public class SingleDependency : ISingleDependency
        {
        }

        public class DependentClass
        {
            public DependentClass(
                IEnumerable<IMultipleDependency> multipleDependencies,
                IEnumerable<IZeroDependency> zeroDependencies,
                IEnumerable<ISingleDependency> singleDependencies)
            {
                MultipleDependencies = multipleDependencies?.ToArray() ??
                                       throw new ArgumentNullException(nameof(multipleDependencies));
                ZeroDependencies = zeroDependencies?.ToArray() ??
                                   throw new ArgumentNullException(nameof(zeroDependencies));
                SingleDependencies = singleDependencies?.ToArray() ??
                                     throw new ArgumentNullException(nameof(singleDependencies));
            }

            public IMultipleDependency[] MultipleDependencies { get; }
            public IZeroDependency[] ZeroDependencies { get; }
            public ISingleDependency[] SingleDependencies { get; }
        }

        public interface IUnsupportedGenericInterface<T>
        {
        }

        public class ClassWithUnsupportedGenericDependency
        {
            public ClassWithUnsupportedGenericDependency(IUnsupportedGenericInterface<SingleDependency> dependency)
            {
            }
        }
    }

    public abstract class EnumerableDependencyTestsBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldResolveTheRegisteredMultipleDependencies()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.MultipleDependencies
                .Should()
                .NotBeNull()
                .And.HaveCount(3)
                .And.ContainSingle(d => d.GetType() == typeof(MultipleDependency1))
                .And.ContainSingle(d => d.GetType() == typeof(MultipleDependency2))
                .And.ContainSingle(d => d.GetType() == typeof(MultipleDependency3));
        }

        [Fact]
        public void ItShouldProvideAnEmptyEnumerableWhereThereAreNoRegistrations()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.ZeroDependencies
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void ItShouldProvideAnEnumerableForASingleDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.SingleDependencies
                .Should()
                .NotBeNull()
                .And.HaveCount(1)
                .And.ContainSingle(d => d.GetType() == typeof(SingleDependency));
        }
    }

    public class WhenRegisteringAClassWithEnumerableDependenciesWithAContext : EnumerableDependencyTestsBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringAClassWithEnumerableDependenciesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<IMultipleDependency, MultipleDependency1>()
                    .RegisterFactory<IMultipleDependency, MultipleDependency2>(c => new MultipleDependency2())
                    .RegisterSingleton<IMultipleDependency, MultipleDependency3>(c => c.UseFactory(f => new MultipleDependency3()))
                    .RegisterFactory(typeof(ISingleDependency), typeof(SingleDependency), f => new SingleDependency())
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenRegisteringAClassWithEnumerableDependenciesWithourAContext : EnumerableDependencyTestsBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringAClassWithEnumerableDependenciesWithourAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<IMultipleDependency, MultipleDependency1>()
                    .RegisterFactory<IMultipleDependency, MultipleDependency2>(() => new MultipleDependency2())
                    .RegisterSingleton<IMultipleDependency, MultipleDependency3>(c => c.UseFactory(() => new MultipleDependency3()))
                    .RegisterFactory(typeof(ISingleDependency), typeof(SingleDependency), () => new SingleDependency())
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }

    public class WhenRegisteringClassWithAnUnsupportedGenericDependency
    {
        private readonly CompositionContainer _composition;

        public WhenRegisteringClassWithAnUnsupportedGenericDependency()
        {
            _composition =
                new RegistrationSetup()
                    .Register<SingleDependency>()
                    .Register<ClassWithUnsupportedGenericDependency>()
                    .Compose();
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                "Failed to get the compositions for the parameter " +
                $"'{typeof(IUnsupportedGenericInterface<SingleDependency>)} dependency' to the constructor of " +
                $"'{typeof(ClassWithUnsupportedGenericDependency)}'. Is there a missing registration mapping?";

            // Act
            Action action = () => _composition.GenerateCode();

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }
}
