// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;

#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
#pragma warning disable CA1806 // Do not ignore method results

namespace Squidex.Infrastructure.Validation;

public static class ValidationExtensions
{
    public static bool IsBetween<TValue>(this TValue value, TValue low, TValue high) where TValue : IComparable
    {
        return Comparer<TValue>.Default.Compare(low, value) <= 0 && Comparer<TValue>.Default.Compare(high, value) >= 0;
    }

    public static bool IsEnumValue<TEnum>(this TEnum value) where TEnum : struct
    {
        try
        {
            return Enum.IsDefined(typeof(TEnum), value);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidRegex(this string value)
    {
        try
        {
            new Regex(value);

            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
