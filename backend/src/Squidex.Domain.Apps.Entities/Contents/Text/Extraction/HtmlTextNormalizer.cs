// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Text.RegularExpressions;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed partial class HtmlTextNormalizer : ITextNormalizer
{
    public int Order => 0;

    public string? Normalize(string text, IField? field)
    {
        if (!IsHtmlField(field) && !LooksLikeHtml(text))
        {
            return text;
        }

        // Replace tags with whitespaces, because inline elements would otherwise merge words.
        var result = HtmlTagRegex().Replace(HtmlCodeRegex().Replace(text, " "), " ");

        return WhitespaceRegex().Replace(WebUtility.HtmlDecode(result), " ").Trim();
    }

    private static bool IsHtmlField(IField? field)
    {
        if (field?.RawProperties is not StringFieldProperties properties)
        {
            return false;
        }

        return properties.Editor is StringFieldEditor.Html or StringFieldEditor.RichText || properties.ContentType == StringContentType.Html;
    }

    private static bool LooksLikeHtml(string text)
    {
        return text.Contains('<', StringComparison.Ordinal) && (text.Contains("</", StringComparison.Ordinal) || text.Contains("/>", StringComparison.Ordinal));
    }

    [GeneratedRegex("<(?<tag>script|style)[^>]*>.*?</\\k<tag>\\s*>", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex HtmlCodeRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();
}
