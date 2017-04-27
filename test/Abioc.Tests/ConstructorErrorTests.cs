// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Xunit;

    public class WhenTryingToCompileUsingAClassWithoutAPublicConstructor
    {
        private readonly RegistrationContext<DefaultConstructionContext> _registrationContext;

        public WhenTryingToCompileUsingAClassWithoutAPublicConstructor()
        {
            _registrationContext = new RegistrationContext<DefaultConstructionContext>()
                .Register<ClassWithoutAPublicConstructor>();
        }

        [Fact]
        public void ItShouldThrowAnIoCCompilationException()
        {
            // Arrange
            string expectedMessage =
                $"The service of type '{typeof(ClassWithoutAPublicConstructor)}' has no public constructors.";
            // Act
            Action action = () =>_registrationContext.Compile(GetType().GetTypeInfo().Assembly);

            // Assert
            action
                .ShouldThrow<IoCCompilationException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenTryingToCompileUsingAClassWithMultiplePublicConstructors
    {
        private readonly RegistrationContext<DefaultConstructionContext> _registrationContext;

        public WhenTryingToCompileUsingAClassWithMultiplePublicConstructors()
        {
            _registrationContext = new RegistrationContext<DefaultConstructionContext>()
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
            Action action = () => _registrationContext.Compile(GetType().GetTypeInfo().Assembly);

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

        private readonly CompilationContext<DefaultConstructionContext> _context;

        public WhenFactoringClassesWithInvalidConstructors()
        {
            _expectedNoPublicConstructor = ClassWithoutAPublicConstructor.Create();
            _expectedMultiplePublicConstructors = new ClassWithMultiplePublicConstructors();

            _context = new RegistrationContext<DefaultConstructionContext>()
                .Register(c => _expectedNoPublicConstructor)
                .Register(c => _expectedMultiplePublicConstructors)
                .Compile(GetType().GetTypeInfo().Assembly);
        }

        [Fact]
        public void ItShouldFactorAClassWithoutAPublicConstructor()
        {
            // Act
            ClassWithoutAPublicConstructor actual = _context.GetService<ClassWithoutAPublicConstructor>();

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
            ClassWithMultiplePublicConstructors actual = _context.GetService<ClassWithMultiplePublicConstructors>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(_expectedMultiplePublicConstructors);
        }
    }

    public class WhenCreatingClassesWithASinglePublicConstructor
    {
        private readonly CompilationContext<DefaultConstructionContext> _context;

        public WhenCreatingClassesWithASinglePublicConstructor()
        {
            _context = new RegistrationContext<DefaultConstructionContext>()
                .Register<SimpleClass1WithoutDependencies>()
                .Register<ClassWithAPrivateAndPublicConstructor>()
                .Register<ClassWithAnInternalAndPublicConstructor>()
                .Compile(GetType().GetTypeInfo().Assembly);
        }

        [Fact]
        public void ItShouldCreateAClassWithAPrivateAndPublicConstructor()
        {
            // Act
            ClassWithAPrivateAndPublicConstructor actual =
                _context.GetService<ClassWithAPrivateAndPublicConstructor>();

            // Assert
            actual.Should().NotBeNull();
            actual.Other.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateAClassWithAnInternalAndPublicConstructor()
        {
            // Act
            ClassWithAnInternalAndPublicConstructor actual =
                _context.GetService<ClassWithAnInternalAndPublicConstructor>();

            // Assert
            actual.Should().NotBeNull();
            actual.Other.Should().NotBeNull();
        }
    }
}
