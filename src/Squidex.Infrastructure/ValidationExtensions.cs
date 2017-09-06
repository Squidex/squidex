// ==========================================================================
//  ValidationExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable RECS0026 // Possible unassigned object created by 'new'

namespace Squidex.Infrastructure
{
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

        public static void Validate(this IValidatable target, Func<string> message)
        {
            var errors = new List<ValidationError>();

            target.Validate(errors);

            if (errors.Any())
            {
                throw new ValidationException(message(), errors);
            }
        }
    }
}
