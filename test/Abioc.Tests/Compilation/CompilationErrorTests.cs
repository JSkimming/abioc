// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abioc.Compilation.CompilationErrorTests;
    using Abioc.Composition;
    using Abioc.Generation;
    using Abioc.Registration;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    namespace CompilationErrorTests
    {
        public class CompilationErrorTestClass1
        {
        }
    }

    public class WhenCompilationFails
    {
        private readonly ITestOutputHelper _output;
        private readonly RegistrationSetup _setup;

        public WhenCompilationFails(ITestOutputHelper output)
        {
            _output = output;
            _setup =
                new RegistrationSetup()
                    .Register<CompilationErrorTestClass1>();
        }


        [Fact]
        public void ItShouldThrowAnInvalidOperationsException()
        {
            // Arrange
            (string code, object[] fieldValues) = _setup.Compose().GenerateCode();
            code = code.Replace("CompilationErrorTestClass1", "CompilationErrorTestClass");
            _output.WriteLine(code);

            // Act
            Action action = () => CodeCompilation.Compile(_setup, code, fieldValues, GetType().GetTypeInfo().Assembly);

            // Assert
            CompilationException exception = action.ShouldThrow<CompilationException>().And;
            _output.WriteLine(exception.ToString());
        }
    }
}
