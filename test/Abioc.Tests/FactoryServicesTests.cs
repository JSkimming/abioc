// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.FactoryServices;
    using FluentAssertions;
    using Xunit;

    namespace FactoryServices
    {
        public interface IService3
        {
        }

        public class Service1
        {
            internal static Service1 CreateService1(DefaultConstructionContext unused) => new Service1();
        }

        public class Service2
        {
            internal static Service2 CreateService2(DefaultConstructionContext unused) => new Service2();
        }

        public class Service3 : IService3
        {
            internal static Service3 CreateService3(DefaultConstructionContext unused) => new Service3();
        }

        public class DependentService
        {
            public DependentService(Service1 service1, Service2 service2, IService3 service3)
            {
                Service1 = service1 ?? throw new ArgumentNullException(nameof(service1));
                Service2 = service2 ?? throw new ArgumentNullException(nameof(service2));
                Service3 = service3 ?? throw new ArgumentNullException(nameof(service3));
            }

            public Service1 Service1 { get; }
            public Service2 Service2 { get; }
            public IService3 Service3 { get; }
        }
    }

    public abstract class WeakOrStrongFactoryServicesTestsBase
    {
        protected WeakOrStrongFactoryServicesTestsBase(
            RegistrationContext<DefaultConstructionContext> registrationContext)
        {
            if (registrationContext == null)
                throw new ArgumentNullException(nameof(registrationContext));

            Context = registrationContext
                .Register(typeof(DependentService))
                .Compile(GetType().GetTypeInfo().Assembly);
        }

        protected CompilationContext<DefaultConstructionContext> Context { get; }

        [Fact]
        public void ItShouldInjectTheFactoredServices()
        {
            // Act
            DependentService actual = Context.GetService<DependentService>();

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
            Service1 actual = Context.GetService<Service1>();

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheSecondFactoredService()
        {
            // Act
            Service2 actual = Context.GetService<Service2>();

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheThirdFactoredService()
        {
            // Act
            IService3 actual = Context.GetService<IService3>();

            // Assert
            actual.Should().NotBeNull();
        }
    }

    public class WhenRegisteringStronglyTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringStronglyTypedFactoryServices()
            : base(new RegistrationContext<DefaultConstructionContext>()
                .Register(Service1.CreateService1)
                .Register(Service2.CreateService2)
                .Register<IService3, Service3>(Service3.CreateService3))
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<DefaultConstructionContext, Service1> expected1 = Service1.CreateService1;
            Func<DefaultConstructionContext, Service2> expected2 = Service2.CreateService2;
            Func<DefaultConstructionContext, Service3> expected3 = Service3.CreateService3;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().BeSameAs(expected1.GetMethodInfo());
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().BeSameAs(expected2.GetMethodInfo());
            Context.SingleMappings[typeof(IService3)].GetMethodInfo().Should().BeSameAs(expected3.GetMethodInfo());
        }
    }

    public class WhenRegisteringWeaklyTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringWeaklyTypedFactoryServices()
            : base(new RegistrationContext<DefaultConstructionContext>()
                .Register(typeof(Service1), Service1.CreateService1)
                .Register(typeof(Service2), Service2.CreateService2)
                .Register(typeof(IService3), typeof(Service3), Service3.CreateService3))
        {
        }

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<DefaultConstructionContext, Service1> notExpected1 = Service1.CreateService1;
            Func<DefaultConstructionContext, Service2> notExpected2 = Service2.CreateService2;
            Func<DefaultConstructionContext, Service3> notExpected3 = Service3.CreateService3;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().NotBeSameAs(notExpected1.GetMethodInfo());
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().NotBeSameAs(notExpected2.GetMethodInfo());
            Context.SingleMappings[typeof(IService3)].GetMethodInfo().Should().NotBeSameAs(notExpected3.GetMethodInfo());
        }
    }

    public class WhenRegisteringMixedTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringMixedTypedFactoryServices()
            : base(new RegistrationContext<DefaultConstructionContext>()
                .Register(Service1.CreateService1)
                .Register(typeof(Service2), Service2.CreateService2)
                .Register<IService3>(Service3.CreateService3))
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<DefaultConstructionContext, Service1> expected = Service1.CreateService1;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().BeSameAs(expected.GetMethodInfo());
        }

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<DefaultConstructionContext, Service2> notExpected = Service2.CreateService2;

            // Act/Assert
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().NotBeSameAs(notExpected.GetMethodInfo());
        }
    }
}
