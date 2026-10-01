// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class MarkdownTextNormalizer : ITextNormalizer
{
    // Markdown can contain HTML, therefore it must be converted before the HTML.
    public int Order => -100;

    public string? Normalize(string text, IField? field)
    {
        if (field?.RawProperties is not StringFieldProperties properties)
        {
            return text;
        }

        if (properties.Editor != StringFieldEditor.Markdown && properties.ContentType != StringContentType.Markdown)
        {
            return text;
        }

        try
        {
            return text.Markdown2Text();
        }
        catch
        {
            return text;
        }
    }
}
