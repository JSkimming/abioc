// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.NestedClassTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace NestedClassTests
    {
        public class OuterClass
        {
            public class NestedClass1
            {
            }

            public class NestedClass2
            {
                public NestedClass2(NestedClass1 nestedClass1)
                {
                    NestedClass1 = nestedClass1 ?? throw new ArgumentNullException(nameof(nestedClass1));
                }

                public NestedClass1 NestedClass1 { get; }
            }

            public class NestedClass3
            {
                public NestedClass3(NestedClass1 nestedClass1, NestedClass2 nestedClass2)
                {
                    NestedClass1 = nestedClass1 ?? throw new ArgumentNullException(nameof(nestedClass1));
                    NestedClass2 = nestedClass2 ?? throw new ArgumentNullException(nameof(nestedClass2));
                }

                public NestedClass1 NestedClass1 { get; }
                public NestedClass2 NestedClass2 { get; }
            }
        }
    }

    public abstract class WhenCreatingNestedClassesBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldCreateTheService()
        {
            // Act
            OuterClass.NestedClass3 actual = GetService<OuterClass.NestedClass3>();

            // Assert
            actual.Should().NotBeNull();
            actual.NestedClass1.Should().NotBeNull();
            actual.NestedClass2.Should().NotBeNull();
        }
    }

    public class WhenCreatingNestedClassesWithAContext : WhenCreatingNestedClassesBase
    {
        private readonly IContainer<int> _container;

        public WhenCreatingNestedClassesWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<OuterClass.NestedClass1>()
                    .Register<OuterClass.NestedClass2>()
                    .Register<OuterClass.NestedClass3>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenCreatingNestedClassesWithoutAContext : WhenCreatingNestedClassesBase
    {
        private readonly IContainer _container;

        public WhenCreatingNestedClassesWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<OuterClass.NestedClass1>()
                    .Register<OuterClass.NestedClass2>()
                    .Register<OuterClass.NestedClass3>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }

    public abstract class WhenFactoringNestedClassesBase
    {
        protected OuterClass.NestedClass1 Expected;

        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldCreateTheServiceUsingTheGivenFactory()
        {
            // Act
            OuterClass.NestedClass3 actual = GetService<OuterClass.NestedClass3>();

            // Assert
            actual.Should().NotBeNull();
            actual.NestedClass1.Should().Be(Expected);
            actual.NestedClass2.Should().NotBeNull();
        }
    }

    public class WhenFactoringNestedClassesWithAContext : WhenFactoringNestedClassesBase
    {
        private readonly IContainer<int> _container;

        public WhenFactoringNestedClassesWithAContext(ITestOutputHelper output)
        {
            Expected = new OuterClass.NestedClass1();

            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory(() => Expected)
                    .Register<OuterClass.NestedClass2>()
                    .Register<OuterClass.NestedClass3>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenFactoringNestedClassesWithoutAContext : WhenFactoringNestedClassesBase
    {
        private readonly IContainer _container;

        public WhenFactoringNestedClassesWithoutAContext(ITestOutputHelper output)
        {
            Expected = new OuterClass.NestedClass1();

            _container =
                new RegistrationSetup()
                    .RegisterFactory(() => Expected)
                    .Register<OuterClass.NestedClass2>()
                    .Register<OuterClass.NestedClass3>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
