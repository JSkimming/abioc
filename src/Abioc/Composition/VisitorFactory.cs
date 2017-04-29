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
    internal static class VisitorFactory
    {
        private static readonly Lazy<Type[]> NonGenericVisitorTypes = new Lazy<Type[]>(GetNonGenericVisitorTypes);

        private static readonly Lazy<Type[]> GenericVisitorTypes = new Lazy<Type[]>(GetGenericVisitorTypes);

        private static readonly ConcurrentDictionary<Type, Func<object>[]> VisitorFactoryCache =
            new ConcurrentDictionary<Type, Func<object>[]>();

        /// <summary>
        /// Creates the visitors that are <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> to the specified type.
        /// </summary>
        /// <typeparam name="TRegistration">The type of visitor.</typeparam>
        /// <returns>
        /// The visitors that are <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/> to the specified type.
        /// </returns>
        public static IEnumerable<IRegistrationVisitor<TRegistration>> CreateVisitors<TRegistration>()
            where TRegistration : class, IRegistration
        {
            Type visitorType = typeof(IRegistrationVisitor<TRegistration>);

            Func<object>[] nonTypedFactories =
                VisitorFactoryCache.GetOrAdd(visitorType, t => CreateVisitorFactories<TRegistration>());

            IEnumerable<Func<IRegistrationVisitor<TRegistration>>> factories =
                nonTypedFactories.Cast<Func<IRegistrationVisitor<TRegistration>>>();

            return factories.Select(f => f());
        }

        private static Func<object>[] CreateVisitorFactories<TRegistration>()
            where TRegistration : class, IRegistration
        {
            TypeInfo visitorTypeInfo = typeof(IRegistrationVisitor<TRegistration>).GetTypeInfo();
            TypeInfo registrationTypeInfo = typeof(TRegistration).GetTypeInfo();

            // Non generic types are much simpler.
            if (!registrationTypeInfo.IsGenericType)
            {
                // Get the non generic types that are assignable to the visitor type.
                IEnumerable<Type> types = NonGenericVisitorTypes.Value.Where(visitorTypeInfo.IsAssignableFrom);

                // Create the visitor factories.
                return types.Select(CreateVisitorFactory<TRegistration>).Cast<Func<object>>().ToArray();
            }

            // Get the generic arguments of the registration type.
            Type[] typeArguments = registrationTypeInfo.GenericTypeArguments;

            // Get the generic types with the same number of type parameters.
            IEnumerable<Type> genericTypeDefinitions =
                GenericVisitorTypes.Value
                    .Where(t => t.GetTypeInfo().GenericTypeParameters.Length == typeArguments.Length);

            // Make the generic types and get the ones that are assignable to the visitor type.
            IEnumerable<Type> genericTypes =
                genericTypeDefinitions
                    .Select(gt => gt.GetTypeInfo().MakeGenericType(typeArguments))
                    .Where(visitorTypeInfo.IsAssignableFrom);

            // Create the visitor factories.
            return genericTypes.Select(CreateVisitorFactory<TRegistration>).Cast<Func<object>>().ToArray();
        }

        private static Func<IRegistrationVisitor<TRegistration>> CreateVisitorFactory<TRegistration>(Type type)
            where TRegistration : class, IRegistration
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ConstructorInfo constructorInfo = type.GetTypeInfo().GetConstructor(Type.EmptyTypes);

            Func<IRegistrationVisitor<TRegistration>> factory =
                Expression.Lambda<Func<IRegistrationVisitor<TRegistration>>>(Expression.New(constructorInfo))
                    .Compile();

            return factory;
        }

        private static IEnumerable<Type> GetAllVisitorTypes()
        {
            Assembly assembly = typeof(IRegistrationVisitor).GetTypeInfo().Assembly;

            IEnumerable<Type> visitorAllTypes =
                from t in assembly.GetTypes().Where(typeof(IRegistrationVisitor).GetTypeInfo().IsAssignableFrom)
                let ti = t.GetTypeInfo()
                where ti.IsClass && !ti.IsAbstract
                select t;

            return visitorAllTypes;
        }

        private static Type[] GetNonGenericVisitorTypes()
        {
            return GetAllVisitorTypes()
                .Where(t => !t.GetTypeInfo().ContainsGenericParameters)
                .OrderBy(t => t.Name)
                .ToArray();
        }

        private static Type[] GetGenericVisitorTypes()
        {
            return GetAllVisitorTypes()
                .Where(t => t.GetTypeInfo().ContainsGenericParameters)
                .OrderBy(t => t.Name)
                .ToArray();
        }

        //// The following is redundant code, though I suspect some of it may make a return, so I'm keeping it here for
        //// reference. I plan to clean this up at some point.

        ////private static Type[] GetTypedVisitorTypes()
        ////{
        ////    IEnumerable<Type> types =
        ////        from t in GetAllVisitorTypes()
        ////        let info = t.GetTypeInfo()
        ////        where info.ContainsGenericParameters
        ////              && info.GenericTypeParameters.Length == 1
        ////              && info.GenericTypeParameters[0].Name == "TImplementation"
        ////        orderby t.Name
        ////        select t;

        ////    return types.ToArray();
        ////}

        ////private static Type[] GetVisitorWithContextTypes()
        ////{
        ////    IEnumerable<Type> types =
        ////        from t in GetAllVisitorTypes()
        ////        let info = t.GetTypeInfo()
        ////        where info.ContainsGenericParameters
        ////              && info.GenericTypeParameters.Length == 1
        ////              && info.GenericTypeParameters[0].Name == "TExtra"
        ////        orderby t.Name
        ////        select t;

        ////    return types.ToArray();
        ////}

        ////private static Type[] GetTypedVisitorWithContextTypes()
        ////{
        ////    IEnumerable<Type> types =
        ////        from t in GetAllVisitorTypes()
        ////        let info = t.GetTypeInfo()
        ////        where info.ContainsGenericParameters
        ////              && info.GenericTypeParameters.Length == 2
        ////              && info.GenericTypeParameters[0].Name == "TExtra"
        ////              && info.GenericTypeParameters[0].Name == "TImplementation"
        ////        orderby t.Name
        ////        select t;

        ////    return types.ToArray();
        ////}

        ////private static Dictionary<Type, List<IRegistrationVisitor>> GetVisitors(
        ////    CompositionContext context)
        ////{
        ////    Assembly assembly = typeof(IRegistrationVisitor).GetTypeInfo().Assembly;
        ////    IEnumerable<Type> visitorTypes =
        ////        from t in assembly.GetTypes().Where(typeof(IRegistrationVisitor).GetTypeInfo().IsAssignableFrom)
        ////        let ti = t.GetTypeInfo()
        ////        where ti.IsClass && !ti.IsAbstract
        ////        select t;

        ////    var visitors = new Dictionary<Type, List<IRegistrationVisitor>>();

        ////    foreach (var visitorType in visitorTypes)
        ////    {
        ////        TypeInfo typeInfo = visitorType.GetTypeInfo();

        ////        IEnumerable<Type> interfaceTypes =
        ////            typeInfo
        ////                .GetInterfaces()
        ////                .Where(i => i.GetTypeInfo().IsGenericType
        ////                            && typeof(IRegistrationVisitor<>) == i.GetGenericTypeDefinition());

        ////        if (typeInfo.ContainsGenericParameters)
        ////        {
        ////            Type[] parameters = typeInfo.GenericTypeParameters;
        ////            Type[] arguments = typeInfo.GenericTypeArguments;
        ////        }

        ////        var visitor = (IRegistrationVisitor)Activator.CreateInstance(visitorType);
        ////        visitor.Initialize(context);

        ////        foreach (Type interfaceType in interfaceTypes)
        ////        {
        ////            List<IRegistrationVisitor> list;
        ////            if (!visitors.TryGetValue(interfaceType, out list))
        ////            {
        ////                list = new List<IRegistrationVisitor>(1);
        ////                visitors[interfaceType] = list;
        ////            }

        ////            list.Add(visitor);
        ////        }
        ////    }

        ////    return visitors;
        ////}

        ////private static IRegistrationVisitor CreateVisitor(Type visitorType)
        ////{
        ////    if (visitorType == null)
        ////        throw new ArgumentNullException(nameof(visitorType));

        ////    var visitor = (IRegistrationVisitor)Activator.CreateInstance(visitorType);
        ////    return visitor;
        ////}

        ////private static IRegistrationVisitor CreateVisitorWithContext(Type visitorType, Type extraType)
        ////{
        ////    if (visitorType == null)
        ////        throw new ArgumentNullException(nameof(visitorType));
        ////    if (extraType == null)
        ////        throw new ArgumentNullException(nameof(extraType));

        ////    TypeInfo typeInfo = visitorType.GetTypeInfo();
        ////    Type genericType = typeInfo.MakeGenericType(extraType);
        ////    return CreateVisitor(genericType);
        ////}

        ////private static IRegistrationVisitor CreateTypedVisitor(Type visitorType, Type implementationType)
        ////{
        ////    if (visitorType == null)
        ////        throw new ArgumentNullException(nameof(visitorType));
        ////    if (implementationType == null)
        ////        throw new ArgumentNullException(nameof(implementationType));

        ////    TypeInfo typeInfo = visitorType.GetTypeInfo();
        ////    Type genericType = typeInfo.MakeGenericType(implementationType);
        ////    return CreateVisitor(genericType);
        ////}

        ////private static IRegistrationVisitor CreateTypedVisitorWithContext(
        ////    Type visitorType,
        ////    Type extraType,
        ////    Type implementationType)
        ////{
        ////    if (visitorType == null)
        ////        throw new ArgumentNullException(nameof(visitorType));
        ////    if (extraType == null)
        ////        throw new ArgumentNullException(nameof(extraType));
        ////    if (implementationType == null)
        ////        throw new ArgumentNullException(nameof(implementationType));

        ////    TypeInfo typeInfo = visitorType.GetTypeInfo();
        ////    Type genericType = typeInfo.MakeGenericType(extraType, implementationType);
        ////    return CreateVisitor(genericType);
        ////}
    }
}
