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
        public class Service1
        {
            internal static Service1 CreateService1(DefaultContructionContext unused) => new Service1();
        }

        public class Service2
        {
            internal static Service2 CreateService2(DefaultContructionContext unused) => new Service2();
        }

        public class DependentService
        {
            public DependentService(Service1 service1, Service2 service2)
            {
                Service1 = service1 ?? throw new ArgumentNullException(nameof(service1));
                Service2 = service2 ?? throw new ArgumentNullException(nameof(service2));
            }

            public Service1 Service1 { get; }
            public Service2 Service2 { get; }
        }
    }

    public abstract class WeakOrStrongFactoryServicesTestsBase
    {
        protected WeakOrStrongFactoryServicesTestsBase(
            RegistrationContext<DefaultContructionContext> registrationContext)
        {
            if (registrationContext == null)
                throw new ArgumentNullException(nameof(registrationContext));

            Context = registrationContext
                .Register(typeof(DependentService))
                .Compile(GetType().GetTypeInfo().Assembly);
        }

        protected CompilationContext<DefaultContructionContext> Context { get; }

        [Fact]
        public void ItShouldInjectTheFactoredServices()
        {
            // Act
            DependentService actual = Context.GetService<DependentService>();

            // Assert
            actual.Should().NotBeNull();
            actual.Service1.Should().NotBeNull();
            actual.Service2.Should().NotBeNull();
        }
    }

    public class WhenRegisteringStronglyTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringStronglyTypedFactoryServices()
            : base(new RegistrationContext<DefaultContructionContext>()
                .Register(Service1.CreateService1)
                .Register(Service2.CreateService2))
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<DefaultContructionContext, Service1> expected1 = Service1.CreateService1;
            Func<DefaultContructionContext, Service2> expected2 = Service2.CreateService2;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().BeSameAs(expected1.GetMethodInfo());
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().BeSameAs(expected2.GetMethodInfo());
        }
    }

    public class WhenRegisteringWeaklyTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringWeaklyTypedFactoryServices()
            : base(new RegistrationContext<DefaultContructionContext>()
                .Register(typeof(Service1), Service1.CreateService1)
                .Register(typeof(Service2), Service2.CreateService2))
        {
        }

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<DefaultContructionContext, Service1> notExpected1 = Service1.CreateService1;
            Func<DefaultContructionContext, Service2> notExpected2 = Service2.CreateService2;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().NotBeSameAs(notExpected1.GetMethodInfo());
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().NotBeSameAs(notExpected2.GetMethodInfo());
        }
    }

    public class WhenRegisteringMixedTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringMixedTypedFactoryServices()
            : base(new RegistrationContext<DefaultContructionContext>()
                .Register(Service1.CreateService1)
                .Register(typeof(Service2), Service2.CreateService2))
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<DefaultContructionContext, Service1> expected = Service1.CreateService1;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().BeSameAs(expected.GetMethodInfo());
        }

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<DefaultContructionContext, Service2> notExpected = Service2.CreateService2;

            // Act/Assert
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().NotBeSameAs(notExpected.GetMethodInfo());
        }
    }
}
