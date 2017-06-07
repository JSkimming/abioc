// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition;
    using Abioc.Registration;
    using Abioc.RegistrationVisitorErrorTests;
    using FluentAssertions;
    using Xunit;

    namespace RegistrationVisitorErrorTests
    {
        internal class TestRegistration : IRegistration
        {
            public Type ImplementationType => typeof(TestRegistration);

            public bool Internal { get; set; }
        }

        internal class VisitorWithoutAPublicConstructor : IRegistrationVisitor<TestRegistration>
        {
            private VisitorWithoutAPublicConstructor()
            {
                throw new NotImplementedException();
            }

            public void Accept(TestRegistration registration)
            {
                throw new NotImplementedException();
            }
        }

        internal class VisitorWithMultiplePublicConstructors : IRegistrationVisitor<TestRegistration>
        {
            public VisitorWithMultiplePublicConstructors()
            {
                throw new NotImplementedException();
            }

            public VisitorWithMultiplePublicConstructors(CompositionContainer container)
            {
                throw new NotImplementedException();
            }

            public void Accept(TestRegistration registration)
            {
                throw new NotImplementedException();
            }
        }

        internal class VisitorWithInvalidConstructorArguments : IRegistrationVisitor<TestRegistration>
        {
            public VisitorWithInvalidConstructorArguments(string invalid1, Guid invalid2)
            {
                throw new NotImplementedException();
            }

            public void Accept(TestRegistration registration)
            {
                throw new NotImplementedException();
            }
        }

    }

    public class WhenExtendingCompositionUsingARegistrationVisitorWithNoPublicConstructor
    {
        private readonly RegistrationSetup _setup;

        public WhenExtendingCompositionUsingARegistrationVisitorWithNoPublicConstructor()
        {
            _setup = new RegistrationSetup()
                .Register(GetType(), c => c.Replace(new TestRegistration()));
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"The registration visitor of type '{typeof(VisitorWithoutAPublicConstructor)}' " +
                "has no public constructors.";

            // Act
            Action action = () => _setup.Compose(typeof(VisitorWithoutAPublicConstructor));

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenExtendingCompositionUsingARegistrationVisitorWithMultiplePublicConstructors
    {
        private readonly RegistrationSetup<int> _setup;

        public WhenExtendingCompositionUsingARegistrationVisitorWithMultiplePublicConstructors()
        {
            _setup = new RegistrationSetup<int>()
                .Register(GetType(), c => c.Replace(new TestRegistration()));
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"The registration visitor of type '{typeof(VisitorWithMultiplePublicConstructors)}' " +
                "has 2 public constructors. There must be just 1.";

            // Act
            Action action = () => _setup.Compose(typeof(VisitorWithMultiplePublicConstructors));

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public class WhenExtendingCompositionUsingARegistrationWithInvalidConstructorArguments
    {
        private readonly RegistrationSetup _setup;

        public WhenExtendingCompositionUsingARegistrationWithInvalidConstructorArguments()
        {
            _setup = new RegistrationSetup()
                .Register(GetType(), c => c.Replace(new TestRegistration()));
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                "The registration visitor constructor must only have parameters of type 'CompositionContainer, " +
                "VisitorManager' but it has 'System.String invalid1, System.Guid invalid2'.";

            // Act
            Action action = () => _setup.Compose(typeof(VisitorWithInvalidConstructorArguments));

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }

    public abstract class WhenExtendingUsingInvalidRegistrationVisitorsBase
    {
        protected Action TestAction;

        [Fact]
        public void ItShouldNotThrowIfTheTheyRemainUnused()
        {
            // Act/Assert
            TestAction.ShouldNotThrow();
        }
    }

    public class WhenExtendingUsingInvalidRegistrationVisitorsWithAContext
        : WhenExtendingUsingInvalidRegistrationVisitorsBase
    {
        public WhenExtendingUsingInvalidRegistrationVisitorsWithAContext()
        {
            var setup = new RegistrationSetup<int>()
                .RegisterFixed(Guid.NewGuid().ToString());

            TestAction = () => setup.Compose(
                typeof(VisitorWithoutAPublicConstructor),
                typeof(VisitorWithMultiplePublicConstructors),
                typeof(VisitorWithInvalidConstructorArguments));
        }
    }

    public class WhenExtendingUsingInvalidRegistrationVisitorsWithoutAContext
        : WhenExtendingUsingInvalidRegistrationVisitorsBase
    {
        public WhenExtendingUsingInvalidRegistrationVisitorsWithoutAContext()
        {
            var setup = new RegistrationSetup()
                .RegisterFixed(Guid.NewGuid().ToString());

            TestAction = () => setup.Compose(
                typeof(VisitorWithoutAPublicConstructor),
                typeof(VisitorWithMultiplePublicConstructors),
                typeof(VisitorWithInvalidConstructorArguments));
        }
    }
}
