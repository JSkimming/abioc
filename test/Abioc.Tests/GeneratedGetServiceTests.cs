// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.RegisterInternalTests;
    using Abioc.Registration;
    using Xunit.Abstractions;

    // The generated GetService method is currently a prototype. Therefore I'm reusing some existing tests to prove it
    // works. These tests will not be necessary if I move over to use the generated get service method, as it will be
    // used everywhere.

    public class WhenUsingTheGeneratedGetServiceMethodWithAContext : RegisterInternalTestsBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenUsingTheGeneratedGetServiceMethodWithAContext(ITestOutputHelper output)
        {
            ExpectedFactoredDependency = new InternalFactoredDependency();
            ExpectedFixedDependency = new InternalFixedDependency();

            _container =
                new RegistrationSetup<int>()
                    .RegisterInternal<IInternalInterfaceDependency, InternalInterfaceDependency>()
                    .RegisterInternal(typeof(InternalConcreteDependency))
                    .RegisterInternal(typeof(InternalFactoredDependency),
                        c => c.UseFactory(typeof(InternalFactoredDependency), () => ExpectedFactoredDependency))
                    .RegisterInternal<InternalFixedDependency>(c => c.UseFixed(ExpectedFixedDependency))
                    .Register<InternalAndExternalDependency>()
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>()
        {
            return (TService)_container.GeneratedGetService(typeof(TService), 1);
        }

        protected override IEnumerable<TService> GetServices<TService>()
        {
            object service = _container.GeneratedGetService(typeof(TService), 1);
            if (service != null)
            {
                yield return (TService)service;
            }
        }
    }

    public class WhenUsingTheGeneratedGetServiceMethodWithoutAContext : RegisterInternalTestsBase
    {
        private readonly AbiocContainer _container;

        public WhenUsingTheGeneratedGetServiceMethodWithoutAContext(ITestOutputHelper output)
        {
            ExpectedFactoredDependency = new InternalFactoredDependency();
            ExpectedFixedDependency = new InternalFixedDependency();

            _container =
                new RegistrationSetup()
                    .RegisterInternal<IInternalInterfaceDependency, InternalInterfaceDependency>()
                    .RegisterInternal(typeof(InternalConcreteDependency))
                    .RegisterInternal(typeof(InternalFactoredDependency),
                        c => c.UseFactory(typeof(InternalFactoredDependency), () => ExpectedFactoredDependency))
                    .RegisterInternal<InternalFixedDependency>(c => c.UseFixed(ExpectedFixedDependency))
                    .Register<InternalAndExternalDependency>()
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>()
        {
            return (TService)_container.GeneratedGetService(typeof(TService));
        }

        protected override IEnumerable<TService> GetServices<TService>()
        {
            object service = _container.GeneratedGetService(typeof(TService));
            if (service != null)
            {
                yield return (TService)service;
            }
        }
    }
}
