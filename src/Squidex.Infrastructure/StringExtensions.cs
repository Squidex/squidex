// ==========================================================================
//  StringExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Squidex.Infrastructure
{
    public static class StringExtensions
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

        public static string ToCamelCase(this string value)
        {
            return char.ToLower(value[0]) + value.Substring(1);
        }

        public static string ToPascalCase(this string value)
        {
            return string.Concat(value.Split(new[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(c => char.ToUpper(c[0]) + c.Substring(1)));
        }

        public static string WithFallback(this string value, string fallback)
        {
            return !string.IsNullOrWhiteSpace(value) ? value.Trim() : fallback;
        }
    }
}
