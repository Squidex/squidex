// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Buffers;
using System.Globalization;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed partial class NoiseTextNormalizer : ITextNormalizer
{
    // Strings without whitespaces above this length are usually tokens, hashes or base64 data.
    private const int MaxWordLength = 64;

    // Same as char.IsWhiteSpace, but vectorized.
    private static readonly SearchValues<char> WhiteSpaces =
        SearchValues.Create(Enumerable.Range(char.MinValue, char.MaxValue + 1).Select(x => (char)x).Where(char.IsWhiteSpace).ToArray());

    // Runs last, because the other normalizers can change the text.
    public int Order => 1000;

    public string? Normalize(string text, IField? field)
    {
        return IsNoise(text) ? null : text;
    }

    public static bool IsNoise(string text)
    {
        // The checks are ordered by costs, most texts are sentences or words and should be handled by the cheap checks.
        var span = text.AsSpan().Trim();
        if (span.Length == 0)
        {
            return true;
        }

        // Anything with whitespaces is considered as normal text.
        if (span.ContainsAny(WhiteSpaces))
        {
            return false;
        }

        if (span.Length > MaxWordLength)
        {
            return true;
        }

        if (IsUrl(span))
        {
            return true;
        }

        // Colors can consist of letters only, e.g. #fff, therefore they are checked before the digits.
        if (span[0] == '#')
        {
            return HexColorRegex().IsMatch(span);
        }

        // Numbers, dates and IDs contain digits.
        if (!span.ContainsAnyInRange('0', '9'))
        {
            return false;
        }

        if (span.Length >= 10 && span[4] == '-' && IsoDateRegex().IsMatch(span))
        {
            return true;
        }

        // Reference IDs, asset IDs and component schema IDs.
        if (Guid.TryParse(span, out _))
        {
            return true;
        }

        return double.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }

    private static bool IsUrl(ReadOnlySpan<char> span)
    {
        // Only the prefix is checked, because a full validation would be expensive and the URL is not indexed anyway.
        if (span.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (span.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return span.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex("^#([0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexColorRegex();

    [GeneratedRegex("^\\d{4}-\\d{2}-\\d{2}(T[0-9:.]+(Z|[+-]\\d{2}:?\\d{2})?)?$")]
    private static partial Regex IsoDateRegex();
}
