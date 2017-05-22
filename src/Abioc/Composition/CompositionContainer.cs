// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition.Compositions;

    /// <summary>
    /// The composition container.
    /// </summary>
    public class CompositionContainer
    {
        private readonly Dictionary<Type, IComposition> _compositions = new Dictionary<Type, IComposition>(32);

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionContainer"/> class.
        /// </summary>
        /// <param name="extraDataType">
        /// The <see cref="Type"/> of the <see cref="ConstructionContext{TExtra}.Extra"/> data.
        /// </param>
        /// <param name="constructionContextType">
        /// The <see cref="Type"/> of the <see cref="ConstructionContext{TExtra}"/>.
        /// </param>
        public CompositionContainer(Type extraDataType = null, Type constructionContextType = null)
        {
            ExtraDataType = extraDataType;
            ConstructionContextType = constructionContextType;
        }

        /// <summary>
        /// Gets the compositions for code generation.
        /// </summary>
        public IReadOnlyDictionary<Type, IComposition> Compositions => _compositions;

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="ConstructionContext{TExtra}.Extra"/> data of the
        /// <see cref="ConstructionContext{TExtra}"/>; otherwise <see langword="null"/> if there is no
        /// <see cref="ConstructionContext{TExtra}"/>.
        /// </summary>
        public Type ExtraDataType { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="ConstructionContext{TExtra}"/>; otherwise
        /// <see langword="null"/> if there is no <see cref="ConstructionContext{TExtra}"/>.
        /// </summary>
        public Type ConstructionContextType { get; }

        /// <summary>
        /// Removes the <see cref="IComposition"/> from the <see cref="Compositions"/> for the specified
        /// <paramref name="type"/> returning the value.
        /// </summary>
        /// <param name="type">The type under which the <see cref="IComposition"/> is keyed.</param>
        /// <returns>The <see cref="IComposition"/> that was removed from the <see cref="Compositions"/>.</returns>
        /// <exception cref="CompositionException">
        /// There is no <see cref="IComposition"/> for the specified <paramref name="type"/>.
        /// </exception>
        public IComposition RemoveComposition(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!_compositions.TryGetValue(type, out IComposition composition))
            {
                throw new CompositionException($"There is no current composition for the type '{type}'.");
            }

            _compositions.Remove(type);
            return composition;
        }

        /// <summary>
        /// Adds the <paramref name="composition"/> to the <see cref="Compositions"/>.
        /// </summary>
        /// <param name="composition">The <see cref="IComposition"/> to add.</param>
        /// <exception cref="CompositionException">
        /// There is already a <see cref="IComposition"/> for the specified
        /// <paramref name="composition"/>.<see cref="IComposition.Type"/> that is not the
        /// default and cannot be superseded by specified <paramref name="composition"/>.
        /// </exception>
        public void AddComposition(IComposition composition)
        {
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            AddComposition(composition.Type, composition);
        }

        /// <summary>
        /// Adds the <paramref name="composition"/> to the <see cref="Compositions"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> that is satisfied by the <paramref name="composition"/></param>
        /// <param name="composition">The <see cref="IComposition"/> to add.</param>
        /// <exception cref="CompositionException">
        /// There is already a <see cref="IComposition"/> for the specified <paramref name="type"/> that is not the
        /// default and cannot be superseded by specified <paramref name="composition"/>.
        /// </exception>
        public void AddComposition(Type type, IComposition composition)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (composition == null)
                throw new ArgumentNullException(nameof(composition));

            if (_compositions.TryGetValue(type, out IComposition existing))
            {
#pragma warning disable SA1119 // Statement must not use unnecessary parenthesis
                if (!(existing is ConstructorComposition constructorComposition) || !constructorComposition.IsDefault)
#pragma warning restore SA1119 // Statement must not use unnecessary parenthesis
                {
                    string message =
                        $"There is already a composition for '{type}', are there multiple registrations. " +
                        $"The Existing composition is '{existing.GetType()}', the new composition is " +
                        $"'{composition.GetType()}'.";
                    throw new CompositionException(message);
                }
            }

            _compositions[type] = composition;
        }
    }
}
