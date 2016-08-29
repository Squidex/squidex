// ==========================================================================
//  Extensions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PinkParrot.Infrastructure
{
    public static class Extensions
    {
        private static readonly Regex SlugRegex = new Regex("^[a-z0-9]+(\\-[a-z0-9]+)*$", RegexOptions.Compiled);

        public static bool IsSlug(this string value)
        {
            return value != null && SlugRegex.IsMatch(value);
        }

        public static bool IsBetween<TValue>(this TValue value, TValue low, TValue high) where TValue : IComparable
        {
            return Comparer<TValue>.Default.Compare(low, value) <= 0 && Comparer<TValue>.Default.Compare(high, value) >= 0;
        }
    }
}
