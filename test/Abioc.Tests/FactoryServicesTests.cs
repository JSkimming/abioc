// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.FactoryServices;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace FactoryServices
    {
        public class InternalService1
        {
        }

        public interface IService3
        {
        }

        public class Service1
        {
            internal static Service1 CreateService1WithContext(ConstructionContext<int> unused) => new Service1();

            internal static Service1 CreateService1WithoutContext() => new Service1();
        }

        public class Service2
        {
            internal static Service2 CreateService2WithContext(ConstructionContext<int> unused) => new Service2();

            internal static Service2 CreateService2WithoutContext() => new Service2();
        }

        public class Service3 : IService3
        {
            internal static Service3 CreateService3WithContext(ConstructionContext<int> unused) => new Service3();

            internal static Service3 CreateService3WithoutContext() => new Service3();
        }

        public class DependentService
        {
            public DependentService(
                InternalService1 internalService1,
                Service1 service1,
                Service2 service2,
                IService3 service3)
            {
                InternalService1 = internalService1 ?? throw new ArgumentNullException(nameof(internalService1));
                Service1 = service1 ?? throw new ArgumentNullException(nameof(service1));
                Service2 = service2 ?? throw new ArgumentNullException(nameof(service2));
                Service3 = service3 ?? throw new ArgumentNullException(nameof(service3));
            }

            public InternalService1 InternalService1 { get; }
            public Service1 Service1 { get; }
            public Service2 Service2 { get; }
            public IService3 Service3 { get; }
        }
    }

    public abstract class WeakOrStrongFactoryServicesTestsBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldInjectTheFactoredServices()
        {
            // Act
            DependentService actual = GetService<DependentService>();

            // Assert
            actual.Should().NotBeNull();
            actual.Service1.Should().NotBeNull();
            actual.Service2.Should().NotBeNull();
            actual.Service3.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheFirstFactoredService()
        {
            // Act
            Service1 actual = GetService<Service1>();

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheSecondFactoredService()
        {
            // Act
            Service2 actual = GetService<Service2>();

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheThirdFactoredService()
        {
            // Act
            IService3 actual = GetService<IService3>();

            // Assert
            actual.Should().NotBeNull();
        }
    }

    public class WhenRegisteringStronglyTypedFactoryServicesWithAContext : WeakOrStrongFactoryServicesTestsBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringStronglyTypedFactoryServicesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory(c => new InternalService1(), c => c.Internal())
                    .RegisterFactory(Service1.CreateService1WithContext)
                    .RegisterFactory(Service2.CreateService2WithContext)
                    .RegisterFactory<IService3, Service3>(Service3.CreateService3WithContext)
                    .Register(typeof(DependentService))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<ConstructionContext<int>, Service1> expected1 = Service1.CreateService1WithContext;
            Func<ConstructionContext<int>, Service2> expected2 = Service2.CreateService2WithContext;
            Func<ConstructionContext<int>, Service3> expected3 = Service3.CreateService3WithContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service1)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected1.GetMethodInfo());

            _container.SingleMappings[typeof(Service2)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected2.GetMethodInfo());

            _container.SingleMappings[typeof(IService3)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected3.GetMethodInfo());
        }
    }

    public class WhenRegisteringStronglyTypedFactoryServicesWithoutAContext : WeakOrStrongFactoryServicesTestsBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringStronglyTypedFactoryServicesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .RegisterFactory(() => new InternalService1(), c => c.Internal())
                    .RegisterFactory(Service1.CreateService1WithoutContext)
                    .RegisterFactory(Service2.CreateService2WithoutContext)
                    .RegisterFactory<IService3, Service3>(Service3.CreateService3WithoutContext)
                    .Register(typeof(DependentService))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<Service1> expected1 = Service1.CreateService1WithoutContext;
            Func<Service2> expected2 = Service2.CreateService2WithoutContext;
            Func<Service3> expected3 = Service3.CreateService3WithoutContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service1)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected1.GetMethodInfo());

            _container.SingleMappings[typeof(Service2)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected2.GetMethodInfo());

            _container.SingleMappings[typeof(IService3)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected3.GetMethodInfo());
        }
    }

    public class WhenRegisteringWeaklyTypedFactoryServicesWithAContext : WeakOrStrongFactoryServicesTestsBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringWeaklyTypedFactoryServicesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory(typeof(InternalService1), c => new InternalService1(), c => c.Internal())
                    .RegisterFactory(typeof(Service1), Service1.CreateService1WithContext)
                    .RegisterFactory(typeof(Service2), Service2.CreateService2WithContext)
                    .RegisterFactory(typeof(IService3), typeof(Service3), Service3.CreateService3WithContext)
                    .Register(typeof(DependentService))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<ConstructionContext<int>, Service1> notExpected1 = Service1.CreateService1WithContext;
            Func<ConstructionContext<int>, Service2> notExpected2 = Service2.CreateService2WithContext;
            Func<ConstructionContext<int>, Service3> notExpected3 = Service3.CreateService3WithContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service1)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected1.GetMethodInfo());

            _container.SingleMappings[typeof(Service2)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected2.GetMethodInfo());

            _container.SingleMappings[typeof(IService3)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected3.GetMethodInfo());
        }
    }

    public class WhenRegisteringWeaklyTypedFactoryServicesWithoutAContext : WeakOrStrongFactoryServicesTestsBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringWeaklyTypedFactoryServicesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .RegisterFactory(typeof(InternalService1), () => new InternalService1(), c => c.Internal())
                    .RegisterFactory(typeof(Service1), Service1.CreateService1WithoutContext)
                    .RegisterFactory(typeof(Service2), Service2.CreateService2WithoutContext)
                    .RegisterFactory(typeof(IService3), typeof(Service3), Service3.CreateService3WithoutContext)
                    .Register(typeof(DependentService))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<Service1> notExpected1 = Service1.CreateService1WithoutContext;
            Func<Service2> notExpected2 = Service2.CreateService2WithoutContext;
            Func<Service3> notExpected3 = Service3.CreateService3WithoutContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service1)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected1.GetMethodInfo());

            _container.SingleMappings[typeof(Service2)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected2.GetMethodInfo());

            _container.SingleMappings[typeof(IService3)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected3.GetMethodInfo());
        }
    }

    public class WhenRegisteringMixedTypedFactoryServicesWithAContext : WeakOrStrongFactoryServicesTestsBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringMixedTypedFactoryServicesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory(typeof(InternalService1), c => new InternalService1(), c => c.Internal())
                    .RegisterFactory(Service1.CreateService1WithContext)
                    .RegisterFactory(typeof(Service2), Service2.CreateService2WithContext)
                    .RegisterFactory<IService3>(Service3.CreateService3WithContext)
                    .Register(typeof(DependentService))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<ConstructionContext<int>, Service1> expected = Service1.CreateService1WithContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service1)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected.GetMethodInfo());
        }

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<ConstructionContext<int>, Service2> notExpected = Service2.CreateService2WithContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service2)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected.GetMethodInfo());
        }
    }

    public class WhenRegisteringMixedTypedFactoryServicesWithoutAContext : WeakOrStrongFactoryServicesTestsBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringMixedTypedFactoryServicesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .RegisterFactory(typeof(InternalService1), () => new InternalService1(), c => c.Internal())
                    .RegisterFactory(Service1.CreateService1WithoutContext)
                    .RegisterFactory(typeof(Service2), Service2.CreateService2WithoutContext)
                    .RegisterFactory<IService3>(Service3.CreateService3WithoutContext)
                    .Register(typeof(DependentService))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<Service1> expected = Service1.CreateService1WithoutContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service1)]
                .GetMethodInfo()
                .Should()
                .BeSameAs(expected.GetMethodInfo());
        }

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<Service2> notExpected = Service2.CreateService2WithoutContext;

            // Act/Assert
            _container.SingleMappings[typeof(Service2)]
                .GetMethodInfo()
                .Should()
                .NotBeSameAs(notExpected.GetMethodInfo());
        }
    }
}
