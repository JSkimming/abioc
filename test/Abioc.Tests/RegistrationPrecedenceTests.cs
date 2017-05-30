// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;
    using Abioc.RegistrationPrecedenceTests;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace RegistrationPrecedenceTests
    {
        public interface IPrecedenceInterface1
        {
        }

        public class PrecedenceClass1 : IPrecedenceInterface1
        {
        }
    }

    public class WhenRegisteringAFactoryAfterAnInterfaceMapping
    {
        private readonly IContainer _container;

        private readonly PrecedenceClass1 _expetcedClass1;

        public WhenRegisteringAFactoryAfterAnInterfaceMapping(ITestOutputHelper output)
        {
            _expetcedClass1 = new PrecedenceClass1();

            _container =
                new RegistrationSetup()
                    .Register<IPrecedenceInterface1, PrecedenceClass1>()
                    .RegisterFactory(() => _expetcedClass1)
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        [Fact]
        public void ItShouldUseTheFactoryTheGetTheClass()
        {
            // Act
            PrecedenceClass1 actual = _container.GetService<PrecedenceClass1>();

            // Assert
            actual.Should().BeSameAs(_expetcedClass1);
        }

        [Fact]
        public void ItShouldUseTheFactoryTheGetTheInterface()
        {
            // Act
            IPrecedenceInterface1 actual = _container.GetService<IPrecedenceInterface1>();

            // Assert
            actual.Should().BeSameAs(_expetcedClass1);
        }
    }

    public class WhenRegisteringAnInterfaceMappingAfterAFactory
    {
        private readonly IContainer _container;

        private readonly PrecedenceClass1 _expetcedClass1;

        public WhenRegisteringAnInterfaceMappingAfterAFactory(ITestOutputHelper output)
        {
            _expetcedClass1 = new PrecedenceClass1();

            _container =
                new RegistrationSetup()
                    .RegisterFactory(() => _expetcedClass1)
                    .Register<IPrecedenceInterface1, PrecedenceClass1>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        [Fact]
        public void ItShouldUseTheFactoryToGetTheClass()
        {
            // Act
            PrecedenceClass1 actual = _container.GetService<PrecedenceClass1>();

            // Assert
            actual.Should().BeSameAs(_expetcedClass1);
        }

        [Fact]
        public void ItShouldUseTheFactoryToGetTheInterface()
        {
            // Act
            IPrecedenceInterface1 actual = _container.GetService<IPrecedenceInterface1>();

            // Assert
            actual.Should().BeSameAs(_expetcedClass1);
        }
    }
}
