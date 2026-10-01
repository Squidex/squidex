// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class HtmlTextNormalizer : ITextNormalizer
{
    public int Order => 0;

    public string? Normalize(string text, IField? field)
    {
        if (!IsHtmlField(field) && !LooksLikeHtml(text))
        {
            return text;
        }

        try
        {
            return text.Html2Text();
        }
        catch
        {
            // The parser fails for some invalid documents.
            return text;
        }
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
        if (!text.Contains('<', StringComparison.Ordinal))
        {
            return false;
        }

        return text.Contains("</", StringComparison.Ordinal) || text.Contains("/>", StringComparison.Ordinal);
    }
}
