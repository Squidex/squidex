// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Squidex.Infrastructure
{
    public static class StringExtensions
    {
        private static readonly Regex EmailRegex = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        private static readonly Regex PropertyNameRegex = new Regex("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public static bool IsEmail(this string? value)
        {
            return value != null && EmailRegex.IsMatch(value);
        }

        public static bool IsPropertyName(this string? value)
        {
            return value != null && PropertyNameRegex.IsMatch(value);
        }

        public static string Or(this string? value, string fallback)
        {
            return !string.IsNullOrWhiteSpace(value) ? value.Trim() : fallback;
        }

        public static string BuildFullUrl(this string baseUrl, string path, bool trailingSlash = false)
        {
            Guard.NotNull(path, nameof(path));

            var url = $"{baseUrl.TrimEnd('/')}/{path.Trim('/')}";

            if (trailingSlash &&
                url.IndexOf("#", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf("?", StringComparison.OrdinalIgnoreCase) < 0 &&
                url.IndexOf(";", StringComparison.OrdinalIgnoreCase) < 0)
            {
                url += "/";
            }

            return url;
        }

        public static string JoinNonEmpty(string separator, params string?[] parts)
        {
            Guard.NotNull(separator, nameof(separator));

            if (parts == null || parts.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(separator, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static string ToIso8601(this DateTime value)
        {
            return value.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
        }
    }
}
