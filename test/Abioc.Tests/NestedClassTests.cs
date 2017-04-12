// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class WhenCreatingNestedClasses
    {
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenCreatingNestedClasses()
        {
            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register<NestedClass1>()
                .Register<NestedClass2>()
                .Register<NestedClass3>()
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldCreateTheService()
        {
            // Act
            NestedClass3 actual = _mappings.GetService<NestedClass3>();

            // Assert
            actual.Should().NotBeNull();
            actual.NestedClass1.Should().NotBeNull();
            actual.NestedClass2.Should().NotBeNull();
        }

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

    public class WhenFactoringNestedClasses
    {
        private readonly WhenCreatingNestedClasses.NestedClass1 _expected;

        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenFactoringNestedClasses()
        {
            _expected = new WhenCreatingNestedClasses.NestedClass1();

            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register(c => _expected)
                .Register<WhenCreatingNestedClasses.NestedClass2>()
                .Register<WhenCreatingNestedClasses.NestedClass3>()
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldCreateTheServiceUsingTheGivenFactory()
        {
            // Act
            WhenCreatingNestedClasses.NestedClass3 actual = _mappings.GetService<WhenCreatingNestedClasses.NestedClass3>();

            // Assert
            actual.Should().NotBeNull();
            actual.NestedClass1.Should().Be(_expected);
            actual.NestedClass2.Should().NotBeNull();
        }
    }
}
