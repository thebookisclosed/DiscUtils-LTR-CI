﻿//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscUtils.Internal
{
    public static class Utilities
    {
        /// <summary>
        /// Converts between two arrays.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the source array.</typeparam>
        /// <typeparam name="U">The type of the elements of the destination array.</typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="func">The function to map from source type to destination type.</param>
        /// <returns>The resultant array.</returns>
        public static U[] Map<T, U>(ICollection<T> source, Func<T, U> func)
        {
            U[] result = new U[source.Count];
            int i = 0;

            foreach (T sVal in source)
            {
                result[i++] = func(sVal);
            }

            return result;
        }

        /// <summary>
        /// Converts between two arrays.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the source array.</typeparam>
        /// <typeparam name="U">The type of the elements of the destination array.</typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="func">The function to map from source type to destination type.</param>
        /// <returns>The resultant array.</returns>
        public static U[] Map<T, U>(IEnumerable<T> source, Converter<T, U> func)
        {
            if (source is T[] array)
            {
                return Array.ConvertAll(array, func);
            }
            else if (source is ICollection<T> collection)
            {
                var result = new U[collection.Count];

                var i = 0;

                foreach (T sVal in source)
                {
                    result[i++] = func(sVal);
                }

                return result;
            }
            else
            {
                List<U> result = new List<U>();

                foreach (T sVal in source)
                {
                    result.Add(func(sVal));
                }

                return result.ToArray();
            }
        }

        /// <summary>
        /// Filters a collection into a new collection.
        /// </summary>
        /// <typeparam name="C">The type of the new collection.</typeparam>
        /// <typeparam name="T">The type of the collection entries.</typeparam>
        /// <param name="source">The collection to filter.</param>
        /// <param name="predicate">The predicate to select which entries are carried over.</param>
        /// <returns>The new collection, containing all entries where the predicate returns <c>true</c>.</returns>
        public static C Filter<C, T>(ICollection<T> source, Func<T, bool> predicate) where C : ICollection<T>, new()
        {
            C result = new C();
            foreach (T val in source)
            {
                if (predicate(val))
                {
                    result.Add(val);
                }
            }

            return result;
        }

        /// <summary>
        /// Indicates if two ranges overlap.
        /// </summary>
        /// <typeparam name="T">The type of the ordinals.</typeparam>
        /// <param name="xFirst">The lowest ordinal of the first range (inclusive).</param>
        /// <param name="xLast">The highest ordinal of the first range (exclusive).</param>
        /// <param name="yFirst">The lowest ordinal of the second range (inclusive).</param>
        /// <param name="yLast">The highest ordinal of the second range (exclusive).</param>
        /// <returns><c>true</c> if the ranges overlap, else <c>false</c>.</returns>
        public static bool RangesOverlap<T>(T xFirst, T xLast, T yFirst, T yLast) where T : IComparable<T>
        {
            return !((xLast.CompareTo(yFirst) <= 0) || (xFirst.CompareTo(yLast) >= 0));
        }
        
        #region Bit Twiddling

        public static bool IsAllZeros(byte[] buffer, int offset, int count)
        {
            int end = offset + count;
            for (int i = offset; i < end; ++i)
            {
                if (buffer[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsPowerOfTwo(uint val)
        {
            if (val == 0)
            {
                return false;
            }

            while ((val & 1) != 1)
            {
                val >>= 1;
            }

            return val == 1;
        }

        public static bool IsPowerOfTwo(long val)
        {
            if (val == 0)
            {
                return false;
            }

            while ((val & 1) != 1)
            {
                val >>= 1;
            }

            return val == 1;
        }

        public static bool AreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            if (ReferenceEquals(a, b))
            {
                return true;
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static ushort BitSwap(ushort value)
        {
            return (ushort)(((value & 0x00FF) << 8) | ((value & 0xFF00) >> 8));
        }

        public static uint BitSwap(uint value)
        {
            return ((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value & 0x00FF0000) >> 8) |
                   ((value & 0xFF000000) >> 24);
        }

        public static ulong BitSwap(ulong value)
        {
            return ((ulong)BitSwap((uint)(value & 0xFFFFFFFF)) << 32) | BitSwap((uint)(value >> 32));
        }

        public static short BitSwap(short value)
        {
            return (short)BitSwap((ushort)value);
        }

        public static int BitSwap(int value)
        {
            return (int)BitSwap((uint)value);
        }

        public static long BitSwap(long value)
        {
            return (long)BitSwap((ulong)value);
        }
        
        #endregion

        #region Path Manipulation

        /// <summary>
        /// Extracts the directory part of a path.
        /// </summary>
        /// <param name="path">The path to process.</param>
        /// <returns>The directory part.</returns>
        public static string GetDirectoryFromPath(string path)
        {
            string trimmed = path.TrimEnd('\\', '/');

            int index = trimmed.LastIndexOf('\\');
            if (index < 0)
            {
                return string.Empty; // No directory, just a file name
            }

            return trimmed.Substring(0, index);
        }

        /// <summary>
        /// Extracts the file part of a path.
        /// </summary>
        /// <param name="path">The path to process.</param>
        /// <returns>The file part of the path.</returns>
        public static string GetFileFromPath(string path)
        {
            string trimmed = path.TrimEnd('\\', '/');

#if NETFRAMEWORK && !NET461_OR_GREATER
            int index = trimmed.LastIndexOf('\\');
            if (index < 0)
            {
                return trimmed; // No directory, just a file name
            }

            return trimmed.Substring(index + 1);
#else
            return Path.GetFileName(trimmed);
#endif
        }

        /// <summary>
        /// Combines two paths.
        /// </summary>
        /// <param name="a">The first part of the path.</param>
        /// <param name="b">The second part of the path.</param>
        /// <returns>The combined path.</returns>
        public static string CombinePaths(string a, string b)
        {
#if NETFRAMEWORK && !NET461_OR_GREATER
            if (string.IsNullOrEmpty(a) || (b.Length > 0 && b[0] == '\\'))
            {
                return b;
            }
            if (string.IsNullOrEmpty(b))
            {
                return a;
            }
            return a.TrimEnd('\\') + '\\' + b.TrimStart('\\');
#else
            return Path.Combine(a, b);
#endif
        }

        /// <summary>
        /// Resolves a relative path into an absolute one.
        /// </summary>
        /// <param name="basePath">The base path to resolve from.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>The absolute path. If no <paramref name="basePath"/> is specified
        /// then relativePath is returned as-is. If <paramref name="relativePath"/>
        /// contains more '..' characters than the base path contains levels of 
        /// directory, the resultant string be the root drive followed by the file name.
        /// If no the basePath starts with '\' (no drive specified) then the returned
        /// path will also start with '\'.
        /// For example: (\TEMP\Foo.txt, ..\..\Bar.txt) gives (\Bar.txt).
        /// </returns>
        public static string ResolveRelativePath(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                return relativePath;
            }

            basePath = Path.GetDirectoryName(basePath);

            var merged = Path.GetFullPath(Path.Combine(basePath, relativePath));

            if (merged.Length > 2 && merged[1] == ':' && merged[2] == '\\')
            {
                return merged.Substring(2);
            }

            return merged;
        }

        public static string ResolvePath(string basePath, string path)
        {
            if (path.Length == 0 || (path[0] != '\\' && path[0] != '/'))
            {
                return ResolveRelativePath(basePath, path);
            }
            return path;
        }

        public static string MakeRelativePath(string path, string basePath)
        {
            var pathElements = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var basePathElements = basePath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (basePathElements.Length > 0 && basePath[basePath.Length - 1] != Path.DirectorySeparatorChar)
            {
                Array.Resize(ref basePathElements, basePathElements.Length - 1);
            }

            // Find first part of paths that don't match
            int i = 0;
            while (i < Math.Min(pathElements.Length - 1, basePathElements.Length))
            {
                if (!pathElements[i].Equals(basePathElements[i], StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                ++i;
            }

            // For each remaining part of the base path, insert '..'
            StringBuilder result = new StringBuilder();
            if (i == basePathElements.Length)
            {
                result.Append(@$".{Path.DirectorySeparatorChar}");
            }
            else if (i < basePathElements.Length)
            {
                for (int j = 0; j < basePathElements.Length - i; ++j)
                {
                    result.Append(@$"..{Path.DirectorySeparatorChar}");
                }
            }

            // For each remaining part of the path, add the path element
            for (int j = i; j < pathElements.Length - 1; ++j)
            {
                result.Append(pathElements[j]);
                result.Append(Path.DirectorySeparatorChar);
            }

            result.Append(pathElements[pathElements.Length - 1]);

            // If the target was a directory, put the terminator back
            if (path[path.Length - 1] == Path.DirectorySeparatorChar)
            {
                result.Append(Path.DirectorySeparatorChar);
            }

            return result.ToString();
        }

#endregion
        
#region Filesystem Support

        /// <summary>
        /// Indicates if a file name matches the 8.3 pattern.
        /// </summary>
        /// <param name="name">The name to test.</param>
        /// <returns><c>true</c> if the name is 8.3, otherwise <c>false</c>.</returns>
        public static bool Is8Dot3(string name)
        {
            if (name.Length > 12)
            {
                return false;
            }

            string[] split = name.Split('.');

            if (split.Length > 2 || split.Length < 1)
            {
                return false;
            }

            if (split[0].Length > 8)
            {
                return false;
            }

            foreach (char ch in split[0])
            {
                if (!Is8Dot3Char(ch))
                {
                    return false;
                }
            }

            if (split.Length > 1)
            {
                if (split[1].Length > 3)
                {
                    return false;
                }

                foreach (char ch in split[1])
                {
                    if (!Is8Dot3Char(ch))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool Is8Dot3Char(char ch)
        {
            return (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || "_^$~!#%£-{}()@'`&".IndexOf(ch) != -1;
        }

        /// <summary>
        /// Converts a 'standard' wildcard file/path specification into a regular expression.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to convert.</param>
        /// <returns>The resultant regular expression.</returns>
        /// <remarks>
        /// The wildcard * (star) matches zero or more characters (including '.'), and ?
        /// (question mark) matches precisely one character (except '.').
        /// </remarks>
        public static Regex ConvertWildcardsToRegEx(string pattern)
        {
            if (!pattern.Contains("."))
            {
                pattern += ".";
            }

            var query = $"^{Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "[^.]")}$";
            return new Regex(query, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        public static FileAttributes FileAttributesFromUnixFileType(this UnixFileType fileType)
        {
            switch (fileType)
            {
                case UnixFileType.Regular:
                    return FileAttributes.Normal;

                case UnixFileType.Directory:
                    return FileAttributes.Directory;

                case UnixFileType.Link:
                    return FileAttributes.ReparsePoint;

                case UnixFileType.Fifo:
                case UnixFileType.Character:
                case UnixFileType.Block:
                case UnixFileType.Socket:
                    return FileAttributes.Device | FileAttributes.System;

                default:
                    return 0;
            }
        }

        public static FileAttributes FileAttributesFromUnixFilePermissions(string name, UnixFilePermissions fileMode, UnixFileType fileType)
        {
            var attr = fileType.FileAttributesFromUnixFileType();

            if (!fileMode.HasFlag(UnixFilePermissions.OwnerWrite))
            {
                attr |= FileAttributes.ReadOnly;
            }

            if (Path.GetFileName(name).StartsWith(".", StringComparison.Ordinal))
            {
                attr |= FileAttributes.Hidden;
            }

            return attr;
        }

        public static UnixFilePermissions UnixFilePermissionsFromFileAttributes(this FileAttributes attributes)
        {
            if ((attributes & FileAttributes.ReadOnly) == 0)
            {
                return UnixFilePermissions.OwnerAll | UnixFilePermissions.GroupAll | UnixFilePermissions.OthersAll;
            }

            return UnixFilePermissions.OwnerRead | UnixFilePermissions.GroupRead | UnixFilePermissions.OthersRead |
                UnixFilePermissions.OwnerExecute | UnixFilePermissions.GroupExecute | UnixFilePermissions.OthersExecute;
        }

        public static string DirectorySeparatorString { get; } = Path.DirectorySeparatorChar.ToString();

        public static bool StartsWithDirectorySeparator(this string path) =>
            path is not null && path.Length > 0 && (path[0] == '/' || path[0] == '\\');

        public static bool EndsWithDirectorySeparator(this string path) =>
            path is not null && path.Length > 0 && (path[path.Length - 1] == '/' || path[path.Length - 1] == '\\');

#endregion
    }
}
