// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition;
    using Abioc.MissingConstructorDependencyTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;

    namespace MissingConstructorDependencyTests
    {
        public class DependencyClass1
        {
        }

        public class DependencyClass2
        {
        }

        public class DependantClass
        {
            public DependantClass(DependencyClass1 dependency1, DependencyClass2 dependency2)
            {
            }
        }
    }

    public class WhenRegisteringAClassWithAMissingDependency
    {
        private readonly RegistrationSetup _setup;

        public WhenRegisteringAClassWithAMissingDependency()
        {
            _setup =
                new RegistrationSetup()
                    .Register<DependencyClass1>()
                    .Register<DependantClass>();
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            string expectedMessage =
                $"Failed to get the compositions for the parameter '{typeof(DependencyClass2)} dependency2' to the " +
                $"constructor of '{typeof(DependantClass)}'. Is there a missing registration mapping?";

            // Act
            Action action = () => _setup.Compose().GenerateCode(_setup.Registrations);

            // Assert
            action
                .ShouldThrow<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }
}
