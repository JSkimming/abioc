// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Composition;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenTryingToCompileUsingAClassWithoutAPublicConstructor
    {
        private readonly RegistrationSetup _setup;

        public WhenTryingToCompileUsingAClassWithoutAPublicConstructor()
        {
            _setup = new RegistrationSetup().Register<ClassWithoutAPublicConstructor>();
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"The service of type '{typeof(ClassWithoutAPublicConstructor)}' has no public constructors.";

            // Act
            Action action = () => _setup.Compose();

            // Assert
            action
                .Should().Throw<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenTryingToCompileUsingAClassWithMultiplePublicConstructors
    {
        private readonly RegistrationSetup _setup;

        public WhenTryingToCompileUsingAClassWithMultiplePublicConstructors()
        {
            _setup = new RegistrationSetup().Register<ClassWithMultiplePublicConstructors>();
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                 $"The service of type '{typeof(ClassWithMultiplePublicConstructors)}' has 2 public constructors. " +
                 "There must be just 1.";

            // Act
            Action action = () => _setup.Compose();

            // Assert
            action
                .Should().Throw<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public abstract class WhenFactoringClassesWithInvalidConstructorsBase
    {
        protected ClassWithoutAPublicConstructor ExpectedNoPublicConstructor;

        protected ClassWithMultiplePublicConstructors ExpectedMultiplePublicConstructors;

        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldFactorAClassWithoutAPublicConstructor()
        {
            // Act
            ClassWithoutAPublicConstructor actual = GetService<ClassWithoutAPublicConstructor>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedNoPublicConstructor);
        }

        [Fact]
        public void ItShouldFactorAClassWithMultiplePublicConstructors()
        {
            // Act
            ClassWithMultiplePublicConstructors actual = GetService<ClassWithMultiplePublicConstructors>();

            // Assert
            actual
                .Should()
                .NotBeNull()
                .And.BeSameAs(ExpectedMultiplePublicConstructors);
        }
    }

    public class WhenFactoringClassesWithInvalidConstructorsWithAContext
        : WhenFactoringClassesWithInvalidConstructorsBase
    {
        private readonly IContainer<int> _container;

        public WhenFactoringClassesWithInvalidConstructorsWithAContext(ITestOutputHelper output)
        {
            ExpectedNoPublicConstructor = ClassWithoutAPublicConstructor.Create();
            ExpectedMultiplePublicConstructors = new ClassWithMultiplePublicConstructors();

            _container =
                new RegistrationSetup<int>()
                    .RegisterFactory(() => ExpectedNoPublicConstructor)
                    .RegisterFactory(c => ExpectedMultiplePublicConstructors)
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenFactoringClassesWithInvalidConstructorsWithoutAContext
        : WhenFactoringClassesWithInvalidConstructorsBase
    {
        private readonly IContainer _container;

        public WhenFactoringClassesWithInvalidConstructorsWithoutAContext(ITestOutputHelper output)
        {
            ExpectedNoPublicConstructor = ClassWithoutAPublicConstructor.Create();
            ExpectedMultiplePublicConstructors = new ClassWithMultiplePublicConstructors();

            _container =
                new RegistrationSetup()
                    .RegisterFactory(() => ExpectedNoPublicConstructor)
                    .RegisterFactory(() => ExpectedMultiplePublicConstructors)
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }

    public abstract class WhenCreatingClassesWithASinglePublicConstructorBase
    {
        protected abstract TService GetService<TService>();

        [Fact]
        public void ItShouldCreateAClassWithAPrivateAndPublicConstructor()
        {
            // Act
            ClassWithAPrivateAndPublicConstructor actual = GetService<ClassWithAPrivateAndPublicConstructor>();

            // Assert
            actual.Should().NotBeNull();
            actual.Other.Should().NotBeNull();
        }

        [Fact]
        public void ItShouldCreateAClassWithAnInternalAndPublicConstructor()
        {
            // Act
            ClassWithAnInternalAndPublicConstructor actual = GetService<ClassWithAnInternalAndPublicConstructor>();

            // Assert
            actual.Should().NotBeNull();
            actual.Other.Should().NotBeNull();
        }
    }

    public class WhenCreatingClassesWithASinglePublicConstructorWithAContext
        : WhenCreatingClassesWithASinglePublicConstructorBase
    {
        private readonly IContainer<int> _container;

        public WhenCreatingClassesWithASinglePublicConstructorWithAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup<int>()
                    .Register<SimpleClass1WithoutDependencies>()
                    .Register<ClassWithAPrivateAndPublicConstructor>()
                    .Register<ClassWithAnInternalAndPublicConstructor>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>(1);
    }

    public class WhenCreatingClassesWithASinglePublicConstructorWithoutAContext
        : WhenCreatingClassesWithASinglePublicConstructorBase
    {
        private readonly IContainer _container;

        public WhenCreatingClassesWithASinglePublicConstructorWithoutAContext(ITestOutputHelper output)
        {
            _container =
                new RegistrationSetup()
                    .Register<SimpleClass1WithoutDependencies>()
                    .Register<ClassWithAPrivateAndPublicConstructor>()
                    .Register<ClassWithAnInternalAndPublicConstructor>()
                    .Construct(GetType().GetTypeInfo().Assembly, out string code);

            output.WriteLine(code);
        }

        protected override TService GetService<TService>() => _container.GetService<TService>();
    }
}
