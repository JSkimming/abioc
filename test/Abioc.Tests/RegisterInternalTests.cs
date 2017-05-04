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
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace RegisterInternalTests
    {
        public interface IInternalInterfaceDependency
        {
        }

        public class InternalInterfaceDependency : IInternalInterfaceDependency
        {
        }

        public class InternalConcreteDependency
        {
        }

        public class InternalFactoredDependency
        {
        }

        public class InternalFixedDependency
        {
        }

        public class InternalAndExternalDependency
        {
        }

        public class DependentClass
        {
            public DependentClass(
                IInternalInterfaceDependency interfaceDependency,
                InternalConcreteDependency concreteDependency,
                InternalFactoredDependency factoredDependency,
                InternalFixedDependency fixedDependency,
                InternalAndExternalDependency internalAndExternalDependency)
            {
                InterfaceDependency = interfaceDependency ??
                                      throw new ArgumentNullException(nameof(interfaceDependency));
                ConcreteDependency = concreteDependency ?? throw new ArgumentNullException(nameof(concreteDependency));
                FactoredDependency = factoredDependency ?? throw new ArgumentNullException(nameof(factoredDependency));
                FixedDependency = fixedDependency ?? throw new ArgumentNullException(nameof(fixedDependency));
                InternalAndExternalDependency = internalAndExternalDependency ??
                                                throw new ArgumentNullException(nameof(internalAndExternalDependency));
            }

            public IInternalInterfaceDependency InterfaceDependency { get; }
            public InternalConcreteDependency ConcreteDependency { get; }
            public InternalFactoredDependency FactoredDependency { get; }
            public InternalFixedDependency FixedDependency { get; }
            public InternalAndExternalDependency InternalAndExternalDependency { get; }
        }
    }

    public abstract class RegisterInternalTestsBase
    {
        protected InternalFactoredDependency ExpectedFactoredDependency;
        protected InternalFixedDependency ExpectedFixedDependency;

        protected abstract TService GetService<TService>();

        protected abstract IEnumerable<TService> GetServices<TService>();

        [Fact]
        public void ItShouldResolveTheInternalInterfaceDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.InterfaceDependency
                .Should()
                .NotBeNull()
                .And.BeOfType<InternalInterfaceDependency>();
        }

        [Fact]
        public void ItShouldResolveTheInternalConcreteDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.ConcreteDependency
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void ItShouldResolveTheInternalFactoryProvidedDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.FactoredDependency
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedFactoredDependency);
        }

        [Fact]
        public void ItShouldResolveTheInternalFixedDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.FixedDependency
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedFixedDependency);
        }

        [Fact]
        public void ItShouldResolveTheInternalAndExternalDependency()
        {
            // Act
            DependentClass actual = GetService<DependentClass>();

            // Assert
            actual.Should().NotBeNull();
            actual.InternalAndExternalDependency
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void ItShouldNotResolveTheInternalInterfaceDependencyExternally()
        {
            // Act
            IEnumerable<IInternalInterfaceDependency> services = GetServices<IInternalInterfaceDependency>();

            // Assert
            services
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void ItShouldNotResolveTheInternalConcreteDependencyExternally()
        {
            // Act
            IEnumerable<InternalConcreteDependency> services = GetServices<InternalConcreteDependency>();

            // Assert
            services
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void ItShouldNotResolveTheInternalFactoryProvidedDependencyExternally()
        {
            // Act
            IEnumerable<InternalFactoredDependency> services = GetServices<InternalFactoredDependency>();

            // Assert
            services
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void ItShouldNotResolveTheInternalFixedDependencyExternally()
        {
            // Act
            IEnumerable<InternalFixedDependency> services = GetServices<InternalFixedDependency>();

            // Assert
            services
                .Should()
                .NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public void ItShouldResolveTheInternalAndExternalDependencyExternally()
        {
            // Act
            InternalAndExternalDependency service = GetService<InternalAndExternalDependency>();

            // Assert
            service.Should().NotBeNull();
        }
    }

    public class WhenRegisteringInternalDependenciesWithAContext : RegisterInternalTestsBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringInternalDependenciesWithAContext(ITestOutputHelper output)
        {
            ExpectedFactoredDependency = new InternalFactoredDependency();
            ExpectedFixedDependency = new InternalFixedDependency();

            _container =
                new RegistrationSetup<int>()
                    .RegisterInternal(typeof(IInternalInterfaceDependency), typeof(InternalInterfaceDependency))
                    .RegisterInternal<InternalConcreteDependency>()
                    .RegisterFactory(c => ExpectedFactoredDependency, c => c.Internal())
                    .Register<InternalFixedDependency>(c => c.UseFixed(ExpectedFixedDependency).Internal())
                    .Register<InternalAndExternalDependency>()
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);

        protected override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>(1);
    }

    public class WhenRegisteringInternalDependenciesWithoutAContext : RegisterInternalTestsBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringInternalDependenciesWithoutAContext(ITestOutputHelper output)
        {
            ExpectedFactoredDependency = new InternalFactoredDependency();
            ExpectedFixedDependency = new InternalFixedDependency();

            _container =
                new RegistrationSetup()
                    .RegisterInternal<IInternalInterfaceDependency, InternalInterfaceDependency>()
                    .RegisterInternal(typeof(InternalConcreteDependency))
                    .RegisterFactory(() => ExpectedFactoredDependency, c => c.Internal())
                    .Register<InternalFixedDependency>(c => c.UseFixed(ExpectedFixedDependency).Internal())
                    .Register<InternalAndExternalDependency>()
                    .Register<DependentClass>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();

        protected override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>();
    }
}
