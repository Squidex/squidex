// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class RichTextFieldTextStrategy : ITextFieldStrategy
{
    public int Order => 0;

    public bool TryExtract(IField? field, JsonValue value, ContentTextWalker walker)
    {
        if (field?.RawProperties is not RichTextFieldProperties || !RichTextNode.TryCreate(value, SquidexRichText.Options, out var node))
        {
            return false;
        }

        walker.AppendText(node.ToText(), field);
        return true;
    }
}
