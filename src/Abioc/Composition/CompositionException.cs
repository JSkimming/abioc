// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;

    /// <summary>
    /// An exception caused during composition.
    /// </summary>
    public class CompositionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionException"/> class with a specified error
        /// <paramref name="message"/> and a reference to the <paramref name="innerException"/> that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a
        /// <see langword="null"/> reference if no inner exception is specified.</param>
        public CompositionException(string message, Exception innerException = null)
            : base(message, innerException)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }
    }
}
