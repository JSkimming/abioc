// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// <see cref="Type"/> extensions helper methods.
    /// </summary>
    internal static class TypeCompositionExtensions
    {
        /// <summary>
        /// Gets the full name of a <paramref name="type"/> that is compilable as part of a method name, e.g. namespace
        /// and nested class delimiters <c>'.'</c> and <c>'+'</c> are replaced with valid characters for a method name.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to return the compilable method name part.</param>
        /// <param name="simpleName">
        /// <p>
        /// If <see langword="true"/> simple method names should be produced; otherwise produce complex method names.
        /// </p>
        /// <p>
        /// A simple method may potentially produce conflicts; though it will produce more readable code.
        /// </p>
        /// </param>
        /// <returns>The full name of a <paramref name="type"/> that is compilable as part of a method name.</returns>
        public static string ToCompileMethodName(this Type type, bool simpleName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            string typeName = simpleName && !type.GetTypeInfo().IsGenericType ? type.Name : type.ToCompileName();
            string name = Regex.Replace(typeName, @"[\.\+<>`\s,]", "_");
            return name;
        }

        /// <summary>
        /// Gets the full name of a <paramref name="type"/> that is compilable, e.g. nested classes have a <c>'+'</c>
        /// delimiter. This is replaced with a <c>'.'</c> to ensure compilation succeeds.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to return the compilable name.</param>
        /// <returns>The full name of a <paramref name="type"/> that is compilable.</returns>
        public static string ToCompileName(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            TypeInfo typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                string name = type.FullName.Replace('+', '.');
                return name;
            }

            Type genericType = typeInfo.GetGenericTypeDefinition();
            int backTickIndex = genericType.FullName.LastIndexOf('`');
            string genericName = genericType.FullName.Remove(backTickIndex).Replace('+', '.');

            IEnumerable<string> genericArguments = typeInfo.GenericTypeArguments.Select(ToCompileName);

            genericName = $"{genericName}<{string.Join(", ", genericArguments)}>";

            return genericName;
        }
    }
}
