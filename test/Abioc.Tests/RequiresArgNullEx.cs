// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abioc.Composition.Compositions;
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

        [Theory, RequiresArgNullExAutoMoq(typeof(AbiocContainer))]
        [Exclude(
            Type = typeof(InjectedSingletonRegistrationCompositionExtension),
            Method = "UseFixed",
            Parameter = "value")]
        [Exclude(
            Type = typeof(RegistrationSetupBase<RegistrationSetup>),
            Method = "RegisterFixed",
            Parameter = "value")]
        [Exclude(
            Type = typeof(ConstructionContextExtensions),
            Parameter = "extra")]
        [Substitute(typeof(AbiocContainer<>), typeof(AbiocContainer<int>))]
        [Substitute(typeof(ConstructionContext<>), typeof(ConstructionContext<int>))]
        [Substitute(typeof(FactoryRegistration<>), typeof(FactoryRegistration<object>))]
        [Substitute(typeof(FactoryRegistrationVisitor<>), typeof(FactoryRegistrationVisitor<object>))]
        [Substitute(typeof(InjectedSingletonComposition<>), typeof(InjectedSingletonComposition<int>))]
        [Substitute(typeof(InjectedSingletonRegistration<>), typeof(InjectedSingletonRegistration<int>))]
        [Substitute(typeof(InjectedSingletonRegistrationVisitor<>), typeof(InjectedSingletonRegistrationVisitor<object>))]
        [Substitute(typeof(RegistrationComposer<>), typeof(RegistrationComposer<object>))]
        [Substitute(typeof(RegistrationComposerExtra<>), typeof(RegistrationComposerExtra<int>))]
        [Substitute(typeof(RegistrationComposerExtra<,>), typeof(RegistrationComposerExtra<int, object>))]
        [Substitute(typeof(RegistrationSetupBase<>), typeof(RegistrationSetupBase<RegistrationSetup>))]
        [Substitute(typeof(RegistrationSetup<>), typeof(RegistrationSetup<int>))]
        [Substitute(typeof(TypedFactoryComposition<>), typeof(TypedFactoryComposition<object>))]
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
