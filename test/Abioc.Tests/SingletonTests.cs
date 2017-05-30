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
        public class ConcreteOnlyDependency1
        {
        }

        public class ConcreteOnlyDependency2
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
                ConcreteOnlyDependency1 concreteOnlyDependency1,
                ConcreteOnlyDependency2 concreteOnlyDependency2,
                IInterfaceOnlyDependency interfaceOnlyDependency,
                IMixedDependency mixedDependencyInferface,
                MixedDependency mixedDependencyClass)
            {
                ConcreteOnlyDependency1 = concreteOnlyDependency1 ??
                                         throw new ArgumentNullException(nameof(concreteOnlyDependency1));
                ConcreteOnlyDependency2 = concreteOnlyDependency2 ??
                                          throw new ArgumentNullException(nameof(concreteOnlyDependency2));
                InterfaceOnlyDependency = interfaceOnlyDependency ??
                                          throw new ArgumentNullException(nameof(interfaceOnlyDependency));
                MixedDependencyInferface = mixedDependencyInferface ??
                                           throw new ArgumentNullException(nameof(mixedDependencyInferface));
                MixedDependencyClass = mixedDependencyClass ??
                                       throw new ArgumentNullException(nameof(mixedDependencyClass));
            }

            public ConcreteOnlyDependency1 ConcreteOnlyDependency1 { get; }
            public ConcreteOnlyDependency2 ConcreteOnlyDependency2 { get; }
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
            actual.ConcreteOnlyDependency1
                .Should()
                .NotBeNull()
                .And.BeSameAs(second.ConcreteOnlyDependency1);
            actual.ConcreteOnlyDependency2
                .Should()
                .NotBeNull()
                .And.BeSameAs(second.ConcreteOnlyDependency2);
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
        private readonly IContainer<int> _container;

        public WhenRegisteringSingletonDependenciesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .RegisterSingleton(typeof(ConcreteOnlyDependency1),
                        c => c.UseFactory(typeof(ConcreteOnlyDependency1), f => new ConcreteOnlyDependency1()))
                    .RegisterSingleton(typeof(ConcreteOnlyDependency2))
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
        private readonly IContainer _container;

        public WhenRegisteringSingletonDependenciesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .RegisterSingleton(typeof(ConcreteOnlyDependency1),
                        c => c.UseFactory(typeof(ConcreteOnlyDependency1), () => new ConcreteOnlyDependency1()))
                    .RegisterSingleton(typeof(ConcreteOnlyDependency2))
                    .RegisterSingleton<IInterfaceOnlyDependency>(c => c.UseFactory(() => new InterfaceOnlyDependency()))
                    .RegisterSingleton<IMixedDependency, MixedDependency>()
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
