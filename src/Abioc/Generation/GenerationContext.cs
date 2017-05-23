// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Generation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abioc.Composition;
    using Abioc.Registration;

    /// <summary>
    /// The context for generated code snippets.
    /// </summary>
    internal class GenerationContext : IGenerationContext
    {
        private readonly List<(string name, Type type, bool requiresContext)> _composeMethodsNames =
            new List<(string, Type, bool)>(32);

        private readonly List<string> _methods = new List<string>(32);

        private readonly List<string> _fields = new List<string>(32);

        private readonly List<(string snippet, object value)> _fieldInitializations = new List<(string, object)>(32);

        private readonly List<string> _additionalInitializations = new List<string>(32);

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationContext"/> class.
        /// </summary>
        /// <param name="registrations">The setup <see cref="Registrations"/>.</param>
        /// <param name="compositions">The <see cref="Compositions"/> for code generation.</param>
        /// <param name="usingSimpleNames">
        /// A value indicating whether simple names should be generated for compose methods.
        /// </param>
        /// <param name="extraDataType">The type of the <see cref="ConstructionContext{T}.Extra"/> data.</param>
        /// <param name="constructionContext">The type of the <see cref="ConstructionContext{T}"/>.</param>
        public GenerationContext(
            IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> registrations,
            IReadOnlyDictionary<Type, IComposition> compositions,
            bool usingSimpleNames,
            string extraDataType = null,
            string constructionContext = null)
        {
            if (registrations == null)
                throw new ArgumentNullException(nameof(registrations));
            if (compositions == null)
                throw new ArgumentNullException(nameof(compositions));

            Registrations = registrations;
            Compositions = compositions;
            UsingSimpleNames = usingSimpleNames;
            ExtraDataType = extraDataType;
            ConstructionContext = constructionContext;
        }

        /// <summary>
        /// Gets the setup <see cref="RegistrationSetupBase{T}.Registrations"/>.
        /// </summary>
        public IReadOnlyDictionary<Type, IReadOnlyList<IRegistration>> Registrations { get; }

        /// <summary>
        /// Gets the compositions for code generation.
        /// </summary>
        public IReadOnlyDictionary<Type, IComposition> Compositions { get; }

        /// <summary>
        /// Gets the type of the <see cref="ConstructionContext{TExtra}.Extra"/> data of the
        /// <see cref="ConstructionContext{TExtra}"/>; otherwise <see langword="null"/> if there is no
        /// <see cref="ConstructionContext{TExtra}"/>.
        /// </summary>
        public string ExtraDataType { get; }

        /// <summary>
        /// Gets the type of the <see cref="ConstructionContext{TExtra}"/>; otherwise <see langword="null"/> if there
        /// is no <see cref="ConstructionContext{TExtra}"/>.
        /// </summary>
        public string ConstructionContext { get; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="ConstructionContext{TExtra}"/> is required for service
        /// resolution.
        /// </summary>
        public bool HasConstructionContext => !string.IsNullOrWhiteSpace(ConstructionContext);

        /// <summary>
        /// Gets the list of generated compose method names. These are the methods returned by the generated
        /// <c>GetCreateMap</c> method.
        /// </summary>
        public IReadOnlyList<(string name, Type type, bool requiresContext)> ComposeMethodsNames =>
            _composeMethodsNames;

        /// <summary>
        /// Gets the list of generated methods.
        /// </summary>
        public IReadOnlyList<string> Methods => _methods;

        /// <summary>
        /// Gets the list of generated fields.
        /// </summary>
        public IReadOnlyList<string> Fields => _fields;

        /// <summary>
        /// Gets the list of field initializations. A field initialization combine a snippet and a value that is
        /// injected in the container constructor.
        /// </summary>
        public IReadOnlyList<(string snippet, object value)> FieldInitializations => _fieldInitializations;

        /// <summary>
        /// Gets the list of additional initialization snippets. These are code snippets that are placed after the
        /// <see cref="FieldInitializations"/> in the constructor.
        /// </summary>
        public IReadOnlyList<string> AdditionalInitializations => _additionalInitializations;

        /// <summary>
        /// Gets a value indicating whether simple names should be generated for compose methods. Simple names may not
        /// be unique, and if not, complex names will be generated.
        /// </summary>
        public bool UsingSimpleNames { get; }

        /// <summary>
        /// Adds a <paramref name="name"/> to the <see cref="ComposeMethodsNames"/> collection.
        /// </summary>
        /// <param name="name">The compose method name to add.</param>
        /// <param name="type">The type of instance provided by the compose method.</param>
        /// <param name="requiresContext">
        /// The value indicating whether the compose method requires a <see cref="ConstructionContext{T}"/>.
        /// </param>
        public void AddComposeMethodsName(string name, Type type, bool requiresContext)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            _composeMethodsNames.Add((name, type, requiresContext));
        }

        /// <summary>
        /// Adds a <paramref name="method"/> to the <see cref="Methods"/> collection.
        /// </summary>
        /// <param name="method">The method to add.</param>
        public void AddMethod(string method)
        {
            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentNullException(nameof(method));

            _methods.Add(method);
        }

        /// <summary>
        /// Adds a <paramref name="field"/> to the <see cref="Fields"/> collection.
        /// </summary>
        /// <param name="field">The field to add.</param>
        public void AddField(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentNullException(nameof(field));

            _fields.Add(field);
        }

        /// <summary>
        /// Adds the <paramref name="snippet"/> to initialize one of the <see cref="Fields"/> with the
        /// <paramref name="value"/>.
        /// </summary>
        /// <param name="snippet">The initialization code snippet.</param>
        /// <param name="value">The value used to initialize one of the <see cref="Fields"/>.</param>
        public void AddFieldInitialization(string snippet, object value)
        {
            if (string.IsNullOrWhiteSpace(snippet))
                throw new ArgumentNullException(nameof(snippet));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _fieldInitializations.Add((snippet, value));
        }

        /// <summary>
        /// Adds an <paramref name="additionalInitialization"/> to the <see cref="AdditionalInitializations"/>
        /// collection. These are code snippets that are placed after the <see cref="FieldInitializations"/> in the
        /// constructor.
        /// </summary>
        /// <param name="additionalInitialization">The additional initialization to add.</param>
        public void AddAdditionalInitialization(string additionalInitialization)
        {
            if (string.IsNullOrWhiteSpace(additionalInitialization))
                throw new ArgumentNullException(nameof(additionalInitialization));

            _additionalInitializations.Add(additionalInitialization);
        }
    }
}
