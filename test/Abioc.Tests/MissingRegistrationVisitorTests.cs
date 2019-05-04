// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition;
    using Abioc.MissingRegistrationVisitorTests;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;

    namespace MissingRegistrationVisitorTests
    {
        internal class RegistrationWithoutAVisitor : IRegistration
        {
            public Type ImplementationType => GetType();

            public bool Internal { get; set; }
        }
    }

    public class WhenComposingAndARegistrationHasNoVisitor
    {
        private readonly RegistrationSetup _setup;

        public WhenComposingAndARegistrationHasNoVisitor()
        {
            _setup =
                new RegistrationSetup()
                    .Register(GetType(), c => c.Replace(new RegistrationWithoutAVisitor()));
        }

        [Fact]
        public void ItShouldThrowACompositionException()
        {
            // Arrange
            // Abioc.Composition.IRegistrationVisitor`1[Abioc.MissingRegistrationVisitorTests.RegistrationWithoutAVisitor]
            string expectedMessage =
                "There are no visitors for registrations of type " +
                $"'{typeof(IRegistrationVisitor<RegistrationWithoutAVisitor>)}'.";

            // Act
            Action action = () => _setup.Compose();

            // Assert
            action
                .Should().Throw<CompositionException>()
                .WithMessage(expectedMessage);
        }
    }
}
