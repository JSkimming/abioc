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
        public interface IGenericInterface<out TResult, in TInput>
        {
            TResult GetValue(TInput input);
        }

        public class TestObject
        {
        }

        public class GetString : IGenericInterface<string, int>
        {
            public string GetValue(int input) => Guid.NewGuid().ToString();
        }

        public class GetObject : IGenericInterface<IList<TestObject>, IEnumerable<IList<object>>>
        {
            public IList<TestObject> GetValue(IEnumerable<IList<object>> input) => new[] { new TestObject() };
        }

        public class GetGuid : IGenericInterface<Guid, TestObject>
        {
            public Guid GetValue(TestObject input) => Guid.NewGuid();
        }
    }

    public abstract class GenericFactoryReturnTestsBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldResolveAllTheServices()
        {
            // Act
            var stringGetter = GetService<IGenericInterface<string, int>>();
            var objectGetter = GetService<IGenericInterface<IList<TestObject>, IEnumerable<IList<object>>>>();
            var guidGetter = GetService<IGenericInterface<Guid, TestObject>>();

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
                    .RegisterFactory<IGenericInterface<string, int>>(c => new GetString())
                    .RegisterFactory(
                        typeof(IGenericInterface<IList<TestObject>, IEnumerable<IList<object>>>),
                        c => new GetObject())
                    .RegisterFactory(
                        typeof(IGenericInterface<Guid, TestObject>),
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
                    .RegisterFactory<IGenericInterface<string, int>>(() => new GetString())
                    .RegisterFactory(
                        typeof(IGenericInterface<IList<TestObject>, IEnumerable<IList<object>>>),
                        () => new GetObject())
                    .RegisterFactory(
                        typeof(IGenericInterface<Guid, TestObject>),
                        () => new GetGuid(),
                        compose => compose.ToSingleton())
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
