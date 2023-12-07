// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Squidex.Infrastructure;

public static partial class StringExtensions
{
    private static readonly Regex RegexEmail = BuildEmailRegex();
    private static readonly Regex RegexProperty = BuildPropertyRegex();
    private static readonly JsonSerializerOptions JsonEscapeOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string JsonEscape(this string value)
    {
        value = JsonSerializer.Serialize(value, JsonEscapeOptions);
        value = value[1..^1];

        return value;
    }

    public static bool IsEmail(this string? value)
    {
        return value != null && RegexEmail.IsMatch(value);
    }

    public static bool IsPropertyName(this string? value)
    {
        return value != null && RegexProperty.IsMatch(value);
    }

    public static string Or(this string? value, string fallback)
    {
        return !string.IsNullOrWhiteSpace(value) ? value.Trim() : fallback;
    }

    public static string JoinNonEmpty(string separator, params string?[] parts)
    {
        Guard.NotNull(separator);

        if (parts is not { Length: > 0 })
        {
            return string.Empty;
        }

        return string.Join(separator, parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    public static string ToIso8601(this DateTime value)
    {
        return value.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
    }

    public static string TrimNonLetterOrDigit(this string value)
    {
        var span = value.AsSpan();

        while (span.Length > 0)
        {
            if (char.IsLetterOrDigit(span[0]))
            {
                break;
            }

            span = span[1..];
        }

        while (span.Length > 0)
        {
            if (char.IsLetterOrDigit(span[^1]))
            {
                break;
            }

            span = span[0..^1];
        }

        if (span.Length == value.Length)
        {
            return value;
        }

        return new string(span);
    }

    public static StringBuilder AppendIfNotEmpty(this StringBuilder sb, char separator)
    {
        if (sb.Length > 0)
        {
            sb.Append(separator);
        }

        return sb;
    }

    public static StringBuilder AppendIfNotEmpty(this StringBuilder sb, string separator)
    {
        if (sb.Length > 0)
        {
            sb.Append(separator);
        }

        return sb;
    }

    [GeneratedRegex("^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-||_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+([a-z]+|\\d|-|\\.{0,1}|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])?([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled, "en-US")]
    private static partial Regex BuildEmailRegex();

    [GeneratedRegex("^[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    private static partial Regex BuildPropertyRegex();
}
