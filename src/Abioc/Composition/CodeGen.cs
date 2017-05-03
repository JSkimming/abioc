// Copyright (c) 2017 James Skimming. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Abioc.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper method for code generation.
    /// </summary>
    internal static class CodeGen
    {
        /// <summary>
        /// Indents the supplied <paramref name="code"/> to the specified <paramref name="depth"/>.
        /// </summary>
        /// <param name="code">The code to indent to the specified <paramref name="depth"/>.</param>
        /// <param name="depth">The depth to indent the <paramref name="code"/>.</param>
        /// <param name="indentation">The indentation characters; the default is 4 spaces.</param>
        /// <returns>The indented <paramref name="code"/>.</returns>
        public static string Indent(string code, int depth = 1, string indentation = "    ")
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));
            if (string.IsNullOrEmpty(indentation))
                throw new ArgumentNullException(nameof(indentation));

            string indentationDepth = string.Empty;
            for (int i = 0; i < depth; ++i)
            {
                indentationDepth = indentationDepth + indentation;
            }

            string newLine = Environment.NewLine;
            string newCode = code.Replace(newLine, newLine + indentationDepth);

            // Split the code into lines.
            string[] lines = newCode.Split(new[] { newLine }, StringSplitOptions.None);

            // Remove trailing whitespace from all the lines.
            newCode = string.Join(newLine, lines.Select(l => l.TrimEnd()));

            // Remove any trailing whitespace (leading whitespace may be indentation)
            newCode = newCode.TrimEnd();

            return newCode;
        }
    }
}
