// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed partial class NoiseTextNormalizer : ITextNormalizer
{
    // Strings without whitespaces above this length are usually tokens, hashes or base64 data.
    private const int MaxWordLength = 64;

    // Runs last, because the other normalizers can change the text.
    public int Order => 1000;

    public string? Normalize(string text, IField? field)
    {
        return IsNoise(text) ? null : text;
    }

    public static bool IsNoise(string text)
    {
        var span = text.AsSpan().Trim();

        if (span.Length == 0)
        {
            return true;
        }

        // Anything with whitespaces is considered as normal text.
        foreach (var c in span)
        {
            if (char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        if (span.Length > MaxWordLength)
        {
            return true;
        }

        // Reference IDs, asset IDs and component schema IDs.
        if (Guid.TryParse(span, out _))
        {
            return true;
        }

        if (double.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        if (HexColorRegex().IsMatch(span) || IsoDateRegex().IsMatch(span))
        {
            return true;
        }

        if (!Uri.TryCreate(span.ToString(), UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "data";
    }

    [GeneratedRegex("^#([0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexColorRegex();

    [GeneratedRegex("^\\d{4}-\\d{2}-\\d{2}(T[0-9:.]+(Z|[+-]\\d{2}:?\\d{2})?)?$")]
    private static partial Regex IsoDateRegex();
}
