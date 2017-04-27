// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;
    using Abioc.RequireConstructionContext;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace RequireConstructionContext
    {
        public class Service1
        {
            public Service1(string extraData = null)
            {
                ExtraData = extraData ?? string.Empty;
            }

            public string ExtraData { get; }

            internal static Service1 CreateService1NoContext() => new Service1();

            internal static Service1 CreateService1WithContext(ConstructionContext<string> context)
                => new Service1(context.Extra);
        }

        public class Service2
        {
            public Service2(string extraData = null)
            {
                ExtraData = extraData ?? string.Empty;
            }

            public string ExtraData { get; }

            internal static Service2 CreateService2NoContext() => new Service2();

            internal static Service2 CreateService2WithContext(ConstructionContext<string> context)
                => new Service2(context.Extra);
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
            RegistrationSetup<string> registrationContext,
            ITestOutputHelper output)
        {
            Context =
                registrationContext
                    .Register(typeof(DependentService))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected AbiocContainer<string> Context { get; }

        [Fact]
        public void ItShouldInjectTheFactoredServices()
        {
            // Arrange
            string expectedExtraData = Guid.NewGuid().ToString();

            // Act
            DependentService actual = Context.GetService<DependentService>(expectedExtraData);

            // Assert
            actual.Should().NotBeNull();
            actual.Service1.Should().NotBeNull();
            actual.Service2.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheFirstFactoredService()
        {
            // Arrange
            string expectedExtraData = Guid.NewGuid().ToString();

            // Act
            Service1 actual = Context.GetService<Service1>(expectedExtraData);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateTheSecondFactoredService()
        {
            // Arrange
            string expectedExtraData = Guid.NewGuid().ToString();

            // Act
            Service2 actual = Context.GetService<Service2>(expectedExtraData);

            // Assert
            actual.Should().NotBeNull();
        }
    }

    public class WhenRegisteringFactoriesThatRequireAConstructionContext : RequireConstructionContextTestsBase
    {
        public WhenRegisteringFactoriesThatRequireAConstructionContext(ITestOutputHelper output)
            : base(new RegistrationSetup<string>()
                    .RegisterFactory(Service1.CreateService1WithContext)
                    .RegisterFactory(typeof(Service2), Service2.CreateService2WithContext),
                output)
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<ConstructionContext<string>, Service1> expected1 = Service1.CreateService1WithContext;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().BeSameAs(expected1.GetMethodInfo());
        }
    }

    public class WhenRegisteringFactoriesThatDoNotRequireAConstructionContext : RequireConstructionContextTestsBase
    {
        public WhenRegisteringFactoriesThatDoNotRequireAConstructionContext(ITestOutputHelper output)
            : base(new RegistrationSetup<string>()
                    .RegisterFactory(Service1.CreateService1NoContext)
                    .RegisterFactory(typeof(Service2), Service2.CreateService2NoContext),
                output)
        {
        }

        [Fact]
        public void ItShouldWrapTheFactoriesToTakeAConstructionContext()
        {
            // Arrange
            Func<ConstructionContext<string>, Service1> notExpected1 = Service1.CreateService1WithContext;
            Func<ConstructionContext<string>, Service2> notExpected2 = Service2.CreateService2WithContext;

            // Act/Assert
            Context.SingleMappings[typeof(Service1)].GetMethodInfo().Should().NotBeSameAs(notExpected1.GetMethodInfo());
            Context.SingleMappings[typeof(Service2)].GetMethodInfo().Should().NotBeSameAs(notExpected2.GetMethodInfo());
        }
    }

    public class WhenRegisteringMixedFactoriesWithSomeRequiringAConstructionContext
        : RequireConstructionContextTestsBase
    {
        public WhenRegisteringMixedFactoriesWithSomeRequiringAConstructionContext(ITestOutputHelper output)
            : base(new RegistrationSetup<string>()
                    .RegisterFactory(Service1.CreateService1WithContext)
                    .RegisterFactory(Service2.CreateService2NoContext),
                output)
        {
        }

        [Fact]
        public void ItShouldUseTheStronglyTypedFactoriesDirectly()
        {
            // Arrange
            Func<ConstructionContext<string>, Service1> expected = Service1.CreateService1WithContext;

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
