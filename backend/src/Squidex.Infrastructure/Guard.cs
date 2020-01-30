// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Squidex.Infrastructure.Validation;

namespace Squidex.Infrastructure
{
    public static class Guard
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidNumber(float target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (float.IsNaN(target) || float.IsPositiveInfinity(target) || float.IsNegativeInfinity(target))
            {
                throw new ArgumentException("Value must be a valid number.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidNumber(double target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (double.IsNaN(target) || double.IsPositiveInfinity(target) || double.IsNegativeInfinity(target))
            {
                throw new ArgumentException("Value must be a valid number.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidSlug(string? target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNullOrEmpty(target, parameterName);

            if (!target!.IsSlug())
            {
                throw new ArgumentException("Target is not a valid slug.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidPropertyName(string? target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNullOrEmpty(target, parameterName);

            if (!target!.IsPropertyName())
            {
                throw new ArgumentException("Target is not a valid property name.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HasType<T>(object? target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target != null && target.GetType() != typeof(T))
            {
                throw new ArgumentException($"The parameter must be of type {typeof(T)}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HasType(object? target, Type? expectedType, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target != null && expectedType != null && target.GetType() != expectedType)
            {
                throw new ArgumentException($"The parameter must be of type {expectedType}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Between<TValue>(TValue target, TValue lower, TValue upper, [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (!target.IsBetween(lower, upper))
            {
                throw new ArgumentException($"Value must be between {lower} and {upper}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enum<TEnum>(TEnum target, [CallerArgumentExpression("target")] string? parameterName = null) where TEnum : struct
        {
            if (!target.IsEnumValue())
            {
                throw new ArgumentException($"Value must be a valid enum type {typeof(TEnum)}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterThan<TValue>(TValue target, TValue lower, [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(lower) <= 0)
            {
                throw new ArgumentException($"Value must be greater than {lower}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterEquals<TValue>(TValue target, TValue lower, [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(lower) < 0)
            {
                throw new ArgumentException($"Value must be greater or equal to {lower}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LessThan<TValue>(TValue target, TValue upper, [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(upper) >= 0)
            {
                throw new ArgumentException($"Value must be less than {upper}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LessEquals<TValue>(TValue target, TValue upper, [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(upper) > 0)
            {
                throw new ArgumentException($"Value must be less or equal to {upper}", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty<TType>(IReadOnlyCollection<TType>? target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNull(target, parameterName);

            if (target != null && target.Count == 0)
            {
                throw new ArgumentException("Collection does not contain an item.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotEmpty(Guid target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target == Guid.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object? target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotDefault<T>(T target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (Equals(target, default(T)!))
            {
                throw new ArgumentException("Value cannot be an the default value.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNullOrEmpty(string? target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNull(target, parameterName);

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("String parameter cannot be null or empty and cannot contain only blanks.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidFileName(string? target, [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNullOrEmpty(target, parameterName);

            if (target.Intersect(Path.GetInvalidFileNameChars()).Any())
            {
                throw new ArgumentException("Value contains an invalid character.", parameterName);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Valid(IValidatable? target, [CallerArgumentExpression("target")] string parameterName, Func<string> message)
        {
            NotNull(target, parameterName);

            target?.Validate(message);
        }
    }
}
