// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using Abioc.Registration;

    /// <summary>
    /// The interface implemented by visitors of a <see cref="IRegistration"/>.
    /// </summary>
    /// <typeparam name="TRegistration">The type of the registration.</typeparam>
    public interface IRegistrationVisitor<in TRegistration> : IRegistrationVisitor
        where TRegistration : class, IRegistration
    {
        /// <summary>
        /// Accepts the <paramref name="registration"/> to visit.
        /// </summary>
        /// <param name="registration">The registration to visit.</param>
        void Accept(TRegistration registration);
    }
}
