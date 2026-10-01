// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class JsonTextStrategy : ITextFieldStrategy
{
    // Fallback for all values, e.g. JSON fields or unknown fields, that indexes all strings.
    public int Order => int.MaxValue;

    public bool TryExtract(IField? field, JsonValue value, ContentTextWalker walker)
    {
        switch (value.Value)
        {
            case string text:
                walker.AppendText(text, field);
                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    walker.AppendValue(item);
                }

                break;
            case JsonObject obj:
                foreach (var (_, item) in obj)
                {
                    walker.AppendValue(item);
                }

                break;
        }

        return true;
    }
}
