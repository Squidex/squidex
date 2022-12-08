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

namespace Squidex.Infrastructure;

public static class Guard
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ValidNumber(float target,
        [CallerArgumentExpression("target")] string? parameterName = null)
    {
        if (float.IsNaN(target) || float.IsPositiveInfinity(target) || float.IsNegativeInfinity(target))
        {
            ThrowHelper.ArgumentException("Value must be a valid number.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("Value must be a valid number.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("Target is not a valid slug.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("Target is not a valid property name.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"The parameter must be of type {typeof(T)}", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"The parameter must be of type {expectedType}", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"Value must be between {lower} and {upper}", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"Value must be a valid enum type {typeof(TEnum)}", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"Value must be greater than {lower}", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"Value must be greater or equal to {lower}", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"Value must be less than {upper}", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException($"Value must be less or equal to {upper}", parameterName);
            return default!;
        }

        return target;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyCollection<TType> NotEmpty<TType>(IReadOnlyCollection<TType>? target,
        [CallerArgumentExpression("target")] string? parameterName = null)
    {
        NotNull(target, parameterName);

        if (target is { Count: 0 })
        {
            ThrowHelper.ArgumentException("Collection does not contain an item.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("Value cannot be empty.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("Value cannot be empty.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentNullException(parameterName);
            return default!;
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
            ThrowHelper.ArgumentNullException(parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("Value cannot be an the default value.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("String parameter cannot be null or empty and cannot contain only blanks.", parameterName);
            return default!;
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
            ThrowHelper.ArgumentException("Value contains an invalid character.", parameterName);
            return default!;
        }

        return target!;
    }
}
