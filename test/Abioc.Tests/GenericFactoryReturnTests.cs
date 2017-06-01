// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.GenericFactoryReturnTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    // These tests demonstrate issue #44 https://github.com/JSkimming/abioc/issues/44
    namespace GenericFactoryReturnTests
    {
        public interface IGenericInterface<out T>
        {
            T GetValue();
        }

        public class TestObject
        {
        }

        public class GetString : IGenericInterface<string>
        {
            public string GetValue() => Guid.NewGuid().ToString();
        }

        public class GetObject : IGenericInterface<TestObject>
        {
            public TestObject GetValue() => new TestObject();
        }

        public class GetGuid : IGenericInterface<Guid>
        {
            public Guid GetValue() => Guid.NewGuid();
        }
    }

    public abstract class GenericFactoryReturnTestsBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldResolveAllTheServices()
        {
            // Act
            IGenericInterface<string> stringGetter = GetService<IGenericInterface<string>>();
            IGenericInterface<TestObject> objectGetter = GetService<IGenericInterface<TestObject>>();
            IGenericInterface<Guid> guidGetter = GetService<IGenericInterface<Guid>>();

            // Assert
            stringGetter.Should().NotBeNull().And.BeOfType<GetString>();
            objectGetter.Should().NotBeNull().And.BeOfType<GetObject>();
            guidGetter.Should().NotBeNull().And.BeOfType<GetGuid>();
        }
    }

    public class WhenFactoringAGenericServiceWithAContext : GenericFactoryReturnTestsBase
    {
        private readonly IContainer<int> _container;

        public WhenFactoringAGenericServiceWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory<IGenericInterface<string>>(c => new GetString())
                    .RegisterFactory(typeof(IGenericInterface<TestObject>), c => new GetObject())
                    .RegisterFactory(
                        typeof(IGenericInterface<Guid>),
                        () => new GetGuid(),
                        compose => compose.ToSingleton())
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenFactoringAGenericServiceWithoutAContext : GenericFactoryReturnTestsBase
    {
        private readonly IContainer _container;

        public WhenFactoringAGenericServiceWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .RegisterFactory<IGenericInterface<string>>(() => new GetString())
                    .RegisterFactory(typeof(IGenericInterface<TestObject>), () => new GetObject())
                    .RegisterFactory(
                        typeof(IGenericInterface<Guid>),
                        () => new GetGuid(),
                        compose => compose.ToSingleton())
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
