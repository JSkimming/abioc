﻿// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The context to maintain the meta-data for an IoC context.
    /// </summary>
    /// <typeparam name="TContructionContext">The type of the context used during service resolution.</typeparam>
    public class RegistrationContext<TContructionContext>
        where TContructionContext : IContructionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationContext{TContructionContext}"/> class.
        /// </summary>
        public RegistrationContext()
        {
            Context = new Dictionary<Type, List<(Type, Func<TContructionContext, object>)>>();
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        public Dictionary<Type, List<(Type implementationType, Func<TContructionContext, object> factory)>> Context
        {
            get;
        }
    }
}
