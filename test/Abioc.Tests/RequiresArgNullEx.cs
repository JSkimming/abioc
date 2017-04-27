// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abioc.Composition.Visitors;
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
        [Exclude(
            Type = typeof(InjectedSingletonRegistrationCompositionExtension),
            Method = "UseFixed",
            Parameter = "value")]
        [Exclude(
            Type = typeof(RegistrationSetupBase<RegistrationSetup>),
            Method = "RegisterFixed",
            Parameter = "value")]
        [Substitute(typeof(AbiocContainer<>), typeof(AbiocContainer<int>))]
        [Substitute(typeof(CompilationContext<>), typeof(CompilationContext<DefaultConstructionContext>))]
        [Substitute(typeof(ConstructionContext<>), typeof(ConstructionContext<int>))]
        [Substitute(typeof(FactoryRegistration<>), typeof(FactoryRegistration<object>))]
        [Substitute(typeof(FactoryRegistrationVisitor<>), typeof(FactoryRegistrationVisitor<object>))]
        [Substitute(typeof(InjectedSingletonRegistration<>), typeof(InjectedSingletonRegistration<int>))]
        [Substitute(typeof(RegistrationComposer<>), typeof(RegistrationComposer<int>))]
        [Substitute(typeof(RegistrationComposer<,>), typeof(RegistrationComposer<int, int>))]
        [Substitute(typeof(RegistrationContext<>), typeof(RegistrationContext<DefaultConstructionContext>))]
        [Substitute(typeof(RegistrationSetupBase<>), typeof(RegistrationSetupBase<RegistrationSetup>))]
        [Substitute(typeof(RegistrationSetup<>), typeof(RegistrationSetup<int>))]
        [Substitute(typeof(TypedFactoryRegistration<>), typeof(TypedFactoryRegistration<object>))]
        [Substitute(typeof(TypedFactoryRegistration<,>), typeof(TypedFactoryRegistration<object, object>))]
        [Substitute(typeof(TypedFactoryRegistrationVisitor<>), typeof(TypedFactoryRegistrationVisitor<object>))]
        [Substitute(typeof(TypedFactoryRegistrationVisitor<,>), typeof(TypedFactoryRegistrationVisitor<object, object>))]
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
