// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.FactoriesUsingTheContainerTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace FactoriesUsingTheContainerTests
    {
        public class Dependency1
        {
        }

        public class Dependent
        {
            public Dependency1 Dependency1 { get; }

            internal Dependent(Dependency1 dependency1)
            {
                Dependency1 = dependency1;
            }
        }
    }

    public class WhenUsingTheConstructionContextToGetServicesInAFactory
    {
        private readonly IContainer<int> _container;

        public WhenUsingTheConstructionContextToGetServicesInAFactory(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<Dependency1>()
                    .RegisterFactory(
                        context => new Dependent(context.Container.GetService<Dependency1>(context.Extra)))
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        [Fact]
        public void ItShouldCreateTheService()
        {
            // Act
            Dependent service = _container.GetService<Dependent>(1);

            // Assert
            service.Should().NotBeNull();
            service.Dependency1.Should().NotBeNull();
        }
    }
}
