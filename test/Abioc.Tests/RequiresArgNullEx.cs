// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abioc.Registration;
    using AutoTest.ArgNullEx;
    using AutoTest.ArgNullEx.Xunit;
    using Xunit;
    using Xunit.Abstractions;

    public class RequiresArgNullEx
    {
        private readonly ITestOutputHelper _output;

        public RequiresArgNullEx(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory, RequiresArgNullExAutoMoq(typeof(IoCCompiler))]
        [Substitute(typeof(CompilationContext<>), typeof(CompilationContext<DefaultContructionContext>))]
        [Substitute(typeof(ContructionContext<>), typeof(ContructionContext<int>))]
        [Substitute(typeof(FactoryRegistration<>), typeof(FactoryRegistration<object>))]
        [Substitute(typeof(RegistrationContext<>), typeof(RegistrationContext<DefaultContructionContext>))]
        [Substitute(typeof(RegistrationSetupBase<>), typeof(RegistrationSetupBase<RegistrationSetup>))]
        [Substitute(typeof(TypedFactoryRegistration<>), typeof(TypedFactoryRegistration<object>))]
        [Substitute(typeof(TypedFactoryRegistration<,>), typeof(TypedFactoryRegistration<object, object>))]
        public Task Abioc(MethodData method)
        {
            // Work around the problem with generic parameters
            if (method.MethodUnderTest.IsGenericMethod && method.MethodUnderTest.ContainsGenericParameters)
            {
                _output.WriteLine("Skipping the test '{0}' as the method is generic.", method.MethodUnderTest);
                return Task.CompletedTask;
            }

            return method.Execute();
        }
    }
}
