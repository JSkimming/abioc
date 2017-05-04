// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.SingletonTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace SingletonTests
    {
        public class ConcreteOnlyDependency
        {
        }

        public interface IInterfaceOnlyDependency
        {
        }

        public class InterfaceOnlyDependency : IInterfaceOnlyDependency
        {
        }

        public interface IMixedDependency
        {
        }

        public class MixedDependency : IMixedDependency
        {
        }

        public class DependentClass
        {
            public DependentClass(
                ConcreteOnlyDependency concreteOnlyDependency,
                IInterfaceOnlyDependency interfaceOnlyDependency,
                IMixedDependency mixedDependencyInferface,
                MixedDependency mixedDependencyClass)
            {
                ConcreteOnlyDependency = concreteOnlyDependency ??
                                         throw new ArgumentNullException(nameof(concreteOnlyDependency));
                InterfaceOnlyDependency = interfaceOnlyDependency ??
                                          throw new ArgumentNullException(nameof(interfaceOnlyDependency));
                MixedDependencyInferface = mixedDependencyInferface ??
                                           throw new ArgumentNullException(nameof(mixedDependencyInferface));
                MixedDependencyClass = mixedDependencyClass ??
                                       throw new ArgumentNullException(nameof(mixedDependencyClass));
            }

            public ConcreteOnlyDependency ConcreteOnlyDependency { get; }
            public IInterfaceOnlyDependency InterfaceOnlyDependency { get; }
            public IMixedDependency MixedDependencyInferface { get; }
            public MixedDependency MixedDependencyClass { get; }
        }
    }

    public abstract class SingletonTestsBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldResolveAConcreteOnlyDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();

            DependentClass second = GetService<DependentClass>();
            actual.ConcreteOnlyDependency
                .Should()
                .NotBeNull()
                .And.BeSameAs(second.ConcreteOnlyDependency);
        }

        [Fact]
        public void ItShouldResolveAnInterfaceOnlyDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();

            DependentClass second = GetService<DependentClass>();
            actual.InterfaceOnlyDependency
                .Should()
                .NotBeNull()
                .And.BeSameAs(second.InterfaceOnlyDependency);
        }

        [Fact]
        public void ItShouldResolveAMixedDependencyInferface()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();

            DependentClass second = GetService<DependentClass>();
            actual.MixedDependencyInferface
                .Should()
                .NotBeNull()
                .And.BeSameAs(actual.MixedDependencyClass)
                .And.BeSameAs(second.MixedDependencyClass)
                .And.BeSameAs(second.MixedDependencyInferface);
        }

        [Fact]
        public void ItShouldResolveAMixedDependencyClass()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();

            DependentClass second = GetService<DependentClass>();
            actual.MixedDependencyClass
                .Should()
                .NotBeNull()
                .And.BeSameAs(actual.MixedDependencyInferface)
                .And.BeSameAs(second.MixedDependencyInferface)
                .And.BeSameAs(second.MixedDependencyClass);
        }
    }

    public class WhenRegisteringSingletonDependenciesWithAContext : SingletonTestsBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringSingletonDependenciesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .RegisterSingleton<ConcreteOnlyDependency>()
                    .RegisterSingleton<IInterfaceOnlyDependency>(c => c.UseFactory(f => new InterfaceOnlyDependency()))
                    .RegisterSingleton<IMixedDependency, MixedDependency>()
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenRegisteringSingletonDependenciesWithoutAContext : SingletonTestsBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringSingletonDependenciesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .RegisterSingleton<ConcreteOnlyDependency>()
                    .RegisterSingleton<IInterfaceOnlyDependency>(c => c.UseFactory(() => new InterfaceOnlyDependency()))
                    .RegisterSingleton<IMixedDependency, MixedDependency>()
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
