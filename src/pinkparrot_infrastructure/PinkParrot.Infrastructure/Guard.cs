// ==========================================================================
//  Guard.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable InvertIf

namespace PinkParrot.Infrastructure
{
    public static class Guard
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidNumber(float target, string parameterName)
        {
            if (float.IsNaN(target) || float.IsPositiveInfinity(target) || float.IsNegativeInfinity(target))
            {
                throw new ArgumentException("Value must be a valid number.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidNumber(double target, string parameterName)
        {
            if (double.IsNaN(target) || double.IsPositiveInfinity(target) || double.IsNegativeInfinity(target))
            {
                throw new ArgumentException("Value must be a valid number.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidSlug(string target, string parameterName)
        {
            NotNullOrEmpty(target, parameterName);

            if (!target.IsSlug())
            {
                throw new ArgumentException("Target is not a valid slug.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HasType<T>(object target, string parameterName)
        {
            NotNull(target, "parameterName");

            if (target.GetType() != typeof(T))
            {
                throw new ArgumentException("The parameter must be of type " + typeof(T), parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Between<TValue>(TValue target, TValue lower, TValue upper, string parameterName) where TValue : IComparable
        {
            if (!target.IsBetween(lower, upper))
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be between {0} and {1}", lower, upper);

                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enum<TEnum>(TEnum target, string parameterName) where TEnum : struct
        {
            if (!target.IsEnumValue())
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be a valid enum type {0}", typeof(TEnum));

                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterThan<TValue>(TValue target, TValue lower, string parameterName) where TValue : IComparable
        {
            if (target.CompareTo(lower) <= 0)
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be greater than {0}", lower);

                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterEquals<TValue>(TValue target, TValue lower, string parameterName) where TValue : IComparable
        {
            if (target.CompareTo(lower) < 0)
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be greater than {0}", lower);

                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LessThan<TValue>(TValue target, TValue upper, string parameterName) where TValue : IComparable
        {
            if (target.CompareTo(upper) >= 0)
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be less than {0}", upper);

                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LessEquals<TValue>(TValue target, TValue upper, string parameterName) where TValue : IComparable
        {
            if (target.CompareTo(upper) > 0)
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be less than {0}", upper);

                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty<TType>(ICollection<TType> enumerable, string parameterName)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            if (enumerable.Count == 0)
            {
                throw new ArgumentException("Collection does not contain an item", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(Guid target, string parameterName)
        {
            if (target == Guid.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object target, string parameterName)
        {
            if (target == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotDefault<T>(T target, string parameterName)
        {
            if (Equals(target, default(T)))
            {
                throw new ArgumentException("Value cannot be an the default value", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmpty(string target, string parameterName, bool allowWhitespacesAtStartOrEnd = true)
        {
            if (target == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("String parameter cannot be null or empty and cannot contain only blanks.", parameterName);
            }

            if (!allowWhitespacesAtStartOrEnd && target.Trim() != target)
            {
                throw new ArgumentException("String cannot start or end with whitespaces", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidFileName(string target, string parameterName)
        {
            NotNullOrEmpty(target, parameterName);

            if (target.Intersect(Path.GetInvalidFileNameChars()).Any())
            {
                throw new ArgumentException("Value contains an invalid character.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsType<T>(object target, string parameterName)
        {
            if (target != null && target.GetType() != typeof(T))
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be of type {0}", typeof(T));

                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsType(object target, Type expectedType, string parameterName)
        {
            if (target != null && expectedType != null && target.GetType() != expectedType)
            {
                var message = string.Format(CultureInfo.CurrentCulture, "Value must be of type {0}", expectedType);

                throw new ArgumentException(message, parameterName);
            }
        }
    }
}
