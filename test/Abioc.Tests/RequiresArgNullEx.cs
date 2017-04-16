// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoTest.ArgNullEx;
    using AutoTest.ArgNullEx.Xunit;
    using Xunit;

    public class RequiresArgNullEx
    {
        [Theory, RequiresArgNullExAutoMoq(typeof(IoCCompiler))]
        [Exclude(Type = typeof(RegistrationContextExtensionsGeneric))]
        [Substitute(typeof(CompilationContext<>), typeof(CompilationContext<DefaultContructionContext>))]
        [Substitute(typeof(RegistrationEntry<>), typeof(RegistrationEntry<DefaultContructionContext>))]
        public Task Abioc(MethodData method)
        {
            return method.Execute();
        }
    }
}
