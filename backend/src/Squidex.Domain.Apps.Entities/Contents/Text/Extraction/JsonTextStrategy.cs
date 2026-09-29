// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class JsonTextStrategy : IFieldTextStrategy
{
    // Fallback for all values, e.g. JSON fields or unknown fields, that indexes all strings.
    public int Order => int.MaxValue;

    public bool TryExtract(IField? field, JsonValue value, TextCollector collector)
    {
        switch (value.Value)
        {
            case string text:
                collector.AppendText(text, field);
                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    collector.AppendValue(item);
                }

                break;
            case JsonObject obj:
                foreach (var (_, item) in obj)
                {
                    collector.AppendValue(item);
                }

                break;
        }

        return true;
    }
}
