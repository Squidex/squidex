// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Squidex.Infrastructure
{
    public static class Extensions
    {
        private static readonly Regex SlugRegex = new Regex("^[a-z0-9]+(\\-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex PropertyNameRegex = new Regex("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$", RegexOptions.Compiled);

        public static bool IsSlug(this string value)
        {
            return value != null && SlugRegex.IsMatch(value);
        }

        public static bool IsPropertyName(this string value)
        {
            return value != null && PropertyNameRegex.IsMatch(value);
        }

        public static bool IsBetween<TValue>(this TValue value, TValue low, TValue high) where TValue : IComparable
        {
            return Comparer<TValue>.Default.Compare(low, value) <= 0 && Comparer<TValue>.Default.Compare(high, value) >= 0;
        }
    }
}
