// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provided the IoC compilation functions.
    /// </summary>
    public static class IoCCompiler
    {
        /// <summary>
        /// TBC.
        /// </summary>
        /// <param name="registration">The registration context.</param>
        public static void Compile(object registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
        }
    }
}
