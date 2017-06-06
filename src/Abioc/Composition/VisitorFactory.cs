// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Abioc.Registration;

    /// <summary>
    /// Factory generator for <see cref="IRegistrationVisitor"/>
    /// </summary>
    internal class VisitorFactory
    {
        private static readonly IReadOnlyList<Type> AllowedParameterTypes = new[]
        {
            typeof(CompositionContainer),
            typeof(VisitorManager),
        };

        private static readonly ConcurrentDictionary<Type, IReadOnlyList<object>> VisitorFactoryCache =
            new ConcurrentDictionary<Type, IReadOnlyList<object>>();

        private static readonly Lazy<IReadOnlyList<Type>> InternalVisitorTypes =
            new Lazy<IReadOnlyList<Type>>(GetInternalVisitorTypes);

        private readonly IReadOnlyList<Type> _visitorTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisitorFactory"/> class.
        /// </summary>
        /// <param name="externalVisitorTypes">
        /// The types of concrete classes that implement <see cref="IRegistrationVisitor"/> from an external assembly.
        /// </param>
        public VisitorFactory(IEnumerable<Type> externalVisitorTypes = null)
        {
            _visitorTypes =
                externalVisitorTypes == null
                    ? InternalVisitorTypes.Value
                    : InternalVisitorTypes.Value.Concat(externalVisitorTypes).Distinct().ToList();
        }

        /// <summary>
        /// Returns all the concrete types in the <paramref name="assembly"/> that implement the
        /// <see cref="IRegistrationVisitor"/> interface.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/>.</param>
        /// <returns>
        /// All the concrete types in the <paramref name="assembly"/> that implement the
        /// <see cref="IRegistrationVisitor"/> interface.
        /// </returns>
        public static IEnumerable<Type> GetAllVisitorTypes(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            IEnumerable<Type> visitorAllTypes =
                from t in assembly.GetTypes().Where(typeof(IRegistrationVisitor).GetTypeInfo().IsAssignableFrom)
                let ti = t.GetTypeInfo()
                where ti.IsClass && !ti.IsAbstract
                select t;

            return visitorAllTypes;
        }

        /// <summary>
        /// Creates the visitors that are <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> to the specified type.
        /// </summary>
        /// <typeparam name="TRegistration">The type of visitor.</typeparam>
        /// <param name="container">The <see cref="CompositionContainer"/>.</param>
        /// <param name="manager">The <see cref="VisitorManager"/>.</param>
        /// <returns>
        /// The visitors that are <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> to the specified type.
        /// </returns>
        public IEnumerable<IRegistrationVisitor<TRegistration>> CreateVisitors<TRegistration>(
            CompositionContainer container,
            VisitorManager manager)
            where TRegistration : class, IRegistration
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            Type visitorType = typeof(IRegistrationVisitor<TRegistration>);

            IReadOnlyList<object> nonTypedFactories =
                VisitorFactoryCache.GetOrAdd(visitorType, t => CreateVisitorFactories<TRegistration>());

            IEnumerable<Func<CompositionContainer, VisitorManager, IRegistrationVisitor<TRegistration>>> factories =
                nonTypedFactories
                    .Cast<Func<CompositionContainer, VisitorManager, IRegistrationVisitor<TRegistration>>>();

            return factories.Select(f => f(container, manager));
        }

        private static Func<CompositionContainer, VisitorManager, IRegistrationVisitor<TRegistration>>
            CreateVisitorFactory<TRegistration>(Type type)
            where TRegistration : class, IRegistration
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ConstructorInfo constructor = GetVisitorConstructor(type);
            IReadOnlyList<ParameterExpression> constructorArguments = GetConstructorArguments(constructor).ToList();
            NewExpression constructorExpression = Expression.New(constructor, constructorArguments);
            IEnumerable<ParameterExpression> lambdaArguments = GetFactoryLambdaArguments(constructorArguments);

            Expression<Func<CompositionContainer, VisitorManager, IRegistrationVisitor<TRegistration>>> lambda
                = Expression.Lambda<Func<CompositionContainer, VisitorManager, IRegistrationVisitor<TRegistration>>>(
                    constructorExpression, lambdaArguments);

            Func<CompositionContainer, VisitorManager, IRegistrationVisitor<TRegistration>> factory =
                lambda.Compile();

            return factory;
        }

        private static ConstructorInfo GetVisitorConstructor(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ConstructorInfo[] constructors =
                type.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            // There must be a public constructor.
            if (constructors.Length == 0)
            {
                string message = $"The registration visitor of type '{type}' has no public constructors.";
                throw new CompositionException(message);
            }

            // There must be just 1 public constructor.
            if (constructors.Length > 1)
            {
                string message = $"The registration visitor of type '{type}' has {constructors.Length:N0} " +
                                 "public constructors. There must be just 1.";
                throw new CompositionException(message);
            }

            ConstructorInfo constructorInfo = constructors[0];
            return constructorInfo;
        }

        private static IEnumerable<ParameterExpression> GetConstructorArguments(
            ConstructorInfo constructor)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            ParameterInfo[] parameterInfos = constructor.GetParameters();

            IReadOnlyList<ParameterInfo> invalidParameters =
                parameterInfos.Where(p => !AllowedParameterTypes.Contains(p.ParameterType)).ToList();

            if (invalidParameters.Count > 0)
            {
                string message =
                    "The registration visitor constructor must only have parameters of type " +
                    $"'{string.Join(", ", AllowedParameterTypes.Select(t => t.Name))}' but it has " +
                    $"'{string.Join(", ", invalidParameters.Select(p => p.ToString()))}'.";
                throw new CompositionException(message);
            }

            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                yield return Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name);
            }
        }

        private static IEnumerable<ParameterExpression> GetFactoryLambdaArguments(
            IReadOnlyCollection<ParameterExpression> constructorArguments)
        {
            if (constructorArguments == null)
                throw new ArgumentNullException(nameof(constructorArguments));

            foreach (Type parameterType in AllowedParameterTypes)
            {
                ParameterExpression argument = constructorArguments.SingleOrDefault(a => a.Type == parameterType);

                if (argument != null)
                {
                    yield return argument;
                }
                else
                {
                    yield return Expression.Parameter(parameterType, "unused_" + Guid.NewGuid().ToString("N"));
                }
            }
        }

        private static IReadOnlyList<Type> GetInternalVisitorTypes()
        {
            Assembly assembly = typeof(IRegistrationVisitor).GetTypeInfo().Assembly;
            IReadOnlyList<Type> internalVisitorTypes = GetAllVisitorTypes(assembly).ToArray();
            return internalVisitorTypes;
        }

        private IReadOnlyList<object> CreateVisitorFactories<TRegistration>()
            where TRegistration : class, IRegistration
        {
            TypeInfo visitorTypeInfo = typeof(IRegistrationVisitor<TRegistration>).GetTypeInfo();
            TypeInfo registrationTypeInfo = typeof(TRegistration).GetTypeInfo();

            // Non generic types are much simpler.
            if (!registrationTypeInfo.IsGenericType)
            {
                // Get the non generic types that are assignable to the visitor type.
                IEnumerable<Type> types = _visitorTypes.Where(visitorTypeInfo.IsAssignableFrom);

                // Create the visitor factories.
                return types.Select(CreateVisitorFactory<TRegistration>).ToArray();
            }

            // Get the generic arguments of the registration type.
            Type[] typeArguments = registrationTypeInfo.GenericTypeArguments;

            // Get the generic types with the same number of type parameters.
            IEnumerable<Type> genericTypeDefinitions =
                _visitorTypes.Where(t => t.GetTypeInfo().GenericTypeParameters.Length == typeArguments.Length);

            // Make the generic types and get the ones that are assignable to the visitor type.
            IEnumerable<Type> genericTypes =
                genericTypeDefinitions
                    .Select(gt => gt.GetTypeInfo().MakeGenericType(typeArguments))
                    .Where(visitorTypeInfo.IsAssignableFrom);

            // Create the visitor factories.
            return genericTypes.Select(CreateVisitorFactory<TRegistration>).ToArray();
        }
    }
}
