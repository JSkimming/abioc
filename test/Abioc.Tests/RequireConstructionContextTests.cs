// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.RequireConstructionContext;
    using FluentAssertions;
    using Xunit;

    namespace RequireConstructionContext
    {
        public class Service1
        {
            internal static Service1 CreateService1NoContext() => new Service1();

            internal static Service1 CreateService1WithContext(DefaultConstructionContext unused) => new Service1();
        }

        public class Service2
        {
            internal static Service2 CreateService2NoContext() => new Service2();

            internal static Service2 CreateService2WithContext(DefaultConstructionContext unused) => new Service2();
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

    public abstract class RequireConstructionContextTestsBase
    {
        protected RequireConstructionContextTestsBase(
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
    }

    public class WhenRegisteringFactoriesThatRequireAConstructionContext : RequireConstructionContextTestsBase
    {
        public WhenRegisteringFactoriesThatRequireAConstructionContext()
            : base(new RegistrationContext<DefaultConstructionContext>()
                .Register(Service1.CreateService1WithContext)
                .Register(typeof(Service2), Service2.CreateService2WithContext))
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<DefaultConstructionContext, Service1> expected1 = Service1.CreateService1WithContext;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().BeSameAs(expected1.GetMethodInfo());
        }
    }

    public class WhenRegisteringFactoriesThatDoNotRequireAConstructionContext : RequireConstructionContextTestsBase
    {
        public WhenRegisteringFactoriesThatDoNotRequireAConstructionContext()
            : base(new RegistrationContext<DefaultConstructionContext>()
                .Register(Service1.CreateService1NoContext)
                .Register(typeof(Service2), Service2.CreateService2NoContext))
        {
        }

        [Fact]
        public void ItShouldWrapTheFactoriesToTakeAConstructionContext()
        {
            // Arrange
            Func<DefaultConstructionContext, Service1> notExpected1 = Service1.CreateService1WithContext;
            Func<DefaultConstructionContext, Service2> notExpected2 = Service2.CreateService2WithContext;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().NotBeSameAs(notExpected1.GetMethodInfo());
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().NotBeSameAs(notExpected2.GetMethodInfo());
        }
    }

    public class WhenRegisteringMixedFactoriesWithSomeRequiringAConstructionContext
        : RequireConstructionContextTestsBase
    {
        public WhenRegisteringMixedFactoriesWithSomeRequiringAConstructionContext()
            : base(new RegistrationContext<DefaultConstructionContext>()
                .Register(Service1.CreateService1WithContext)
                .Register(Service2.CreateService2NoContext))
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<DefaultConstructionContext, Service1> expected = Service1.CreateService1WithContext;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().BeSameAs(expected.GetMethodInfo());
        }

        [Fact]
        public void ItShouldWrapTheWeaklyTypedFactories()
        {
            // Arrange
            Func<Service2> notExpected = Service2.CreateService2NoContext;

            // Act/Assert
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().NotBeSameAs(notExpected.GetMethodInfo());
        }
    }
}
