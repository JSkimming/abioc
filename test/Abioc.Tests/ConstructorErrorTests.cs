// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class WhenTryingToCompileUsingAClassWithoutAPublicConstructor
    {
        private readonly RegistrationContext<DefaultContructionContext> _registrationContext;

        public WhenTryingToCompileUsingAClassWithoutAPublicConstructor()
        {
            _registrationContext = new RegistrationContext<DefaultContructionContext>()
                .Register<ClassWithoutAPublicConstructor>();
        }

        [Fact]
        public void ItShouldThrowAnIoCCompilationException()
        {
            // Arrange
            string expectedMessage =
                $"The service of type '{typeof(ClassWithoutAPublicConstructor)}' has no public constructors.";
            // Act
            Action action = () =>_registrationContext.Compile(GetType().Assembly);

            // Assert
            action
                .ShouldThrow<IoCCompilationException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenTryingToCompileUsingAClassWithMultiplePublicConstructors
    {
        private readonly RegistrationContext<DefaultContructionContext> _registrationContext;

        public WhenTryingToCompileUsingAClassWithMultiplePublicConstructors()
        {
            _registrationContext = new RegistrationContext<DefaultContructionContext>()
                .Register<ClassWithMultiplePublicConstructors>();
        }

        [Fact]
        public void ItShouldThrowAnIoCCompilationException()
        {
            // Arrange
            string expectedMessage =
                 $"The service of type '{typeof(ClassWithMultiplePublicConstructors)}' has 2 public constructors. " +
                 "There must be just 1.";
            // Act
            Action action = () => _registrationContext.Compile(GetType().Assembly);

            // Assert
            action
                .ShouldThrow<IoCCompilationException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenFactoringClassesWithInvalidConstructors
    {
        private readonly ClassWithoutAPublicConstructor _expectedNoPublicConstructor;
        private readonly ClassWithMultiplePublicConstructors _expectedMultiplePublicConstructors;

        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenFactoringClassesWithInvalidConstructors()
        {
            _expectedNoPublicConstructor = ClassWithoutAPublicConstructor.Create();
            _expectedMultiplePublicConstructors = new ClassWithMultiplePublicConstructors();

            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register(c => _expectedNoPublicConstructor)
                .Register(c => _expectedMultiplePublicConstructors)
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldFactorAClassWithoutAPublicConstructor()
        {
            // Act
            ClassWithoutAPublicConstructor actual = _mappings.GetService<ClassWithoutAPublicConstructor>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(_expectedNoPublicConstructor);
        }

        [Fact]
        public void ItShouldFactorAClassWithMultiplePublicConstructors()
        {
            // Act
            ClassWithMultiplePublicConstructors actual = _mappings.GetService<ClassWithMultiplePublicConstructors>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(_expectedMultiplePublicConstructors);
        }
    }

    public class WhenCreatingClassesWithASinglePublicConstructor
    {
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<Func<DefaultContructionContext, object>>> _mappings;

        public WhenCreatingClassesWithASinglePublicConstructor()
        {
            _mappings = new RegistrationContext<DefaultContructionContext>()
                .Register<SimpleClass1WithoutDependencies>()
                .Register<ClassWithAPrivateAndPublicConstructor>()
                .Register<ClassWithAnInternalAndPublicConstructor>()
                .Compile(GetType().Assembly);
        }

        [Fact]
        public void ItShouldCreateAClassWithAPrivateAndPublicConstructor()
        {
            // Act
            ClassWithAPrivateAndPublicConstructor actual = _mappings.GetService<ClassWithAPrivateAndPublicConstructor>();

            // Assert
            actual.Should().NotBeNull();
            actual.Other.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateAClassWithAnInternalAndPublicConstructor()
        {
            // Act
            ClassWithAnInternalAndPublicConstructor actual = _mappings.GetService<ClassWithAnInternalAndPublicConstructor>();

            // Assert
            actual.Should().NotBeNull();
            actual.Other.Should().NotBeNull();
        }
    }
}
