// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Filter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoTest.ArgNullEx.Filter;

    /// <summary>
    /// Filters out delegate types.
    /// </summary>
    internal class NotDelegate : FilterBase, ITypeFilter
    {
        /// <summary>
        /// Filters out delegate types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><see langword="true"/> if the <paramref name="type"/> should be excluded;
        /// otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> parameter is <see langword="null"/>.
        /// </exception>
        public bool ExcludeType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return typeof(Delegate).IsAssignableFrom(type);
        }
    }
}
