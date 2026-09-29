// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class ArrayFieldTextStrategy : IFieldTextStrategy
{
    public int Order => 0;

    public bool TryExtract(IField? field, JsonValue value, TextCollector collector)
    {
        if (field is not IArrayField arrayField || value.Value is not JsonArray items)
        {
            return false;
        }

        foreach (var item in items)
        {
            if (item.Value is JsonObject obj)
            {
                collector.AppendObject(obj, name => arrayField.FieldsByName.GetValueOrDefault(name));
            }
        }

        return true;
    }
}
