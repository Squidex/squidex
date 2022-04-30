// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Squidex.Infrastructure.Validation;
using Squidex.Text;

namespace Squidex.Infrastructure
{
    public static class Guard
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ValidNumber(float target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (float.IsNaN(target) || float.IsPositiveInfinity(target) || float.IsNegativeInfinity(target))
            {
                throw new ArgumentException("Value must be a valid number.", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ValidNumber(double target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (double.IsNaN(target) || double.IsPositiveInfinity(target) || double.IsNegativeInfinity(target))
            {
                throw new ArgumentException("Value must be a valid number.", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidSlug(string? target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNullOrEmpty(target, parameterName);

            if (!target!.IsSlug())
            {
                throw new ArgumentException("Target is not a valid slug.", parameterName);
            }

            return target!;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidPropertyName(string? target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNullOrEmpty(target, parameterName);

            if (!target!.IsPropertyName())
            {
                throw new ArgumentException("Target is not a valid property name.", parameterName);
            }

            return target!;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? HasType<T>(object? target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target != null && target.GetType() != typeof(T))
            {
                throw new ArgumentException($"The parameter must be of type {typeof(T)}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? HasType(object? target, Type? expectedType,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target != null && expectedType != null && target.GetType() != expectedType)
            {
                throw new ArgumentException($"The parameter must be of type {expectedType}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue Between<TValue>(TValue target, TValue lower, TValue upper,
            [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (!target.IsBetween(lower, upper))
            {
                throw new ArgumentException($"Value must be between {lower} and {upper}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum Enum<TEnum>(TEnum target,
            [CallerArgumentExpression("target")] string? parameterName = null) where TEnum : struct
        {
            if (!target.IsEnumValue())
            {
                throw new ArgumentException($"Value must be a valid enum type {typeof(TEnum)}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GreaterThan<TValue>(TValue target, TValue lower,
            [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(lower) <= 0)
            {
                throw new ArgumentException($"Value must be greater than {lower}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GreaterEquals<TValue>(TValue target, TValue lower,
            [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(lower) < 0)
            {
                throw new ArgumentException($"Value must be greater or equal to {lower}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue LessThan<TValue>(TValue target, TValue upper,
            [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(upper) >= 0)
            {
                throw new ArgumentException($"Value must be less than {upper}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue LessEquals<TValue>(TValue target, TValue upper,
            [CallerArgumentExpression("target")] string? parameterName = null) where TValue : IComparable
        {
            if (target.CompareTo(upper) > 0)
            {
                throw new ArgumentException($"Value must be less or equal to {upper}", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyCollection<TType> NotEmpty<TType>(IReadOnlyCollection<TType>? target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNull(target, parameterName);

            if (target != null && target.Count == 0)
            {
                throw new ArgumentException("Collection does not contain an item.", parameterName);
            }

            return target!;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid NotEmpty(Guid target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target == Guid.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DomainId NotEmpty(DomainId target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target == DomainId.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue NotNull<TValue>(TValue? target,
            [CallerArgumentExpression("target")] string? parameterName = null) where TValue : class
        {
            if (target == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? NotNull(object? target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue NotDefault<TValue>(TValue target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            if (Equals(target, default(TValue)!))
            {
                throw new ArgumentException("Value cannot be an the default value.", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NotNullOrEmpty(string? target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNull(target, parameterName);

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("String parameter cannot be null or empty and cannot contain only blanks.", parameterName);
            }

            return target;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ValidFileName(string? target,
            [CallerArgumentExpression("target")] string? parameterName = null)
        {
            NotNullOrEmpty(target, parameterName);

            if (target != null && target.Intersect(Path.GetInvalidFileNameChars()).Any())
            {
                throw new ArgumentException("Value contains an invalid character.", parameterName);
            }

            return target!;
        }
    }
}
