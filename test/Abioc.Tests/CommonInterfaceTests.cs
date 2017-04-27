// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class WhenRegisteringTwoClassThatImplementTheSameInterfaceBase
    {
        public abstract TService GetService<TService>();

        public abstract IEnumerable<TService> GetServices<TService>();

        [Fact]
        public void ItShouldCreateBothImplementationsOfTheSameInterface()
        {
            // Act
            IReadOnlyList<ISimpleInterface> actual = GetServices<ISimpleInterface>().ToList();

            // Assert
            actual.Should()
                .HaveCount(2)
                .And.Contain(s => s is SimpleClass1WithoutDependencies)
                .And.Contain(s => s is SimpleClass2WithoutDependencies);
        }

        [Fact]
        public void ItShouldThrowADiExceptionIfGettingAsASingleService()
        {
            // Arrange
            string expectedMessage =
                $"There are multiple registered factories to create services of type '{typeof(ISimpleInterface)}'.";

            // Act
            Action action = () => GetService<ISimpleInterface>();

            // Assert
            action
                .ShouldThrow<DiException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenRegisteringTwoClassThatImplementTheSameInterfaceWithAContext
        : WhenRegisteringTwoClassThatImplementTheSameInterfaceBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenRegisteringTwoClassThatImplementTheSameInterfaceWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<ISimpleInterface, SimpleClass1WithoutDependencies>()
                    .Register<ISimpleInterface, SimpleClass2WithoutDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        public override TService GetService<TService>() => _container.GetService<TService>(1);

        public override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>(1);
    }

    public class WhenRegisteringTwoClassThatImplementTheSameInterfaceWithoutAContext
        : WhenRegisteringTwoClassThatImplementTheSameInterfaceBase
    {
        private readonly AbiocContainer _container;

        public WhenRegisteringTwoClassThatImplementTheSameInterfaceWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<ISimpleInterface, SimpleClass1WithoutDependencies>()
                    .Register<ISimpleInterface, SimpleClass2WithoutDependencies>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        public override TService GetService<TService>() => _container.GetService<TService>();

        public override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>();
    }

    public abstract class WhenFactoringTwoClassThatImplementTheSameInterfaceBase
    {
        protected SimpleClass1WithoutDependencies _expected1;

        protected SimpleClass2WithoutDependencies _expected2;

        public abstract TService GetService<TService>();

        public abstract IEnumerable<TService> GetServices<TService>();

        [Fact]
        public void ItShouldCreateBothImplementationsOfTheSameInterface()
        {
            // Act
            IReadOnlyList<ISimpleInterface> actual = GetServices<ISimpleInterface>().ToList();

            // Assert
            actual.Should()
                .HaveCount(2)
                .And.Contain(_expected1)
                .And.Contain(_expected2);
        }

        [Fact]
        public void ItShouldThrowADiExceptionIfGettingAsASingleService()
        {
            // Arrange
            string expectedMessage =
                $"There are multiple registered factories to create services of type '{typeof(ISimpleInterface)}'.";

            // Act
            Action action = () => GetService<ISimpleInterface>();

            // Assert
            action
                .ShouldThrow<DiException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenFactoringTwoClassThatImplementTheSameInterfaceWithAContext
        : WhenFactoringTwoClassThatImplementTheSameInterfaceBase
    {
        private readonly AbiocContainer<int> _container;

        public WhenFactoringTwoClassThatImplementTheSameInterfaceWithAContext(ITestOutputHelper output)
        {
            _expected1 = new SimpleClass1WithoutDependencies();
            _expected2 = new SimpleClass2WithoutDependencies();

            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory<ISimpleInterface, SimpleClass1WithoutDependencies>(c => _expected1)
                    .RegisterFactory<ISimpleInterface, SimpleClass2WithoutDependencies>(() => _expected2)
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        public override TService GetService<TService>() => _container.GetService<TService>(1);

        public override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>(1);
    }

    public class WhenFactoringTwoClassThatImplementTheSameInterfaceWithoutAContext
        : WhenFactoringTwoClassThatImplementTheSameInterfaceBase
    {
        private readonly AbiocContainer _container;

        public WhenFactoringTwoClassThatImplementTheSameInterfaceWithoutAContext(ITestOutputHelper output)
        {
            _expected1 = new SimpleClass1WithoutDependencies();
            _expected2 = new SimpleClass2WithoutDependencies();

            _container =
                new RegistrationSetup()
                    .RegisterFactory<ISimpleInterface, SimpleClass1WithoutDependencies>(() => _expected1)
                    .RegisterFactory<ISimpleInterface, SimpleClass2WithoutDependencies>(() => _expected2)
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        public override TService GetService<TService>() => _container.GetService<TService>();

        public override IEnumerable<TService> GetServices<TService>() => _container.GetServices<TService>();
    }
}
