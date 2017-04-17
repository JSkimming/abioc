// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.FactoryServices;
    using FluentAssertions;
    using Xunit;

    namespace FactoryServices
    {
        public class Service1
        {
        }

        public class Service2
        {
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
                .Compile(GetType().Assembly);
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
                .Register(c => new Service1())
                .Register(c => new Service2()))
        {
        }
    }

    public class WhenRegisteringWeaklyTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringWeaklyTypedFactoryServices()
            : base(new RegistrationContext<DefaultContructionContext>()
                .Register(typeof(Service1), c => new Service1())
                .Register(typeof(Service2), c => new Service2()))
        {
        }
    }

    public class WhenRegisteringMixedTypedFactoryServices : WeakOrStrongFactoryServicesTestsBase
    {
        public WhenRegisteringMixedTypedFactoryServices()
            : base(new RegistrationContext<DefaultContructionContext>()
                .Register(c => new Service1())
                .Register(typeof(Service2), c => new Service2()))
        {
        }
    }
}
