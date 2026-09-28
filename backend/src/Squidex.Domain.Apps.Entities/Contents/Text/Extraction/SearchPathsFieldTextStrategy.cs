// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class SearchPathsFieldTextStrategy : IFieldTextStrategy
{
    // Search paths are configured explicitly and therefore override all other strategies.
    public int Order => -1000;

    public bool TryExtract(IField? field, JsonValue value, FieldTextContext context)
    {
        if (field?.RawProperties.SearchPaths is not { Count: > 0 } paths)
        {
            return false;
        }

        foreach (var path in paths)
        {
            AppendPath(value, ParsePath(path), 0, context);
        }

        return true;
    }

    private static void AppendPath(JsonValue value, string[] path, int index, FieldTextContext context)
    {
        if (index == path.Length)
        {
            context.AppendValue(value);
            return;
        }

        switch (value.Value)
        {
            case JsonArray array:
                // Arrays are traversed implicitly, so that users do not have to specify the index.
                foreach (var item in array)
                {
                    AppendPath(item, path, index, context);
                }

                break;
            case JsonObject obj when obj.TryGetValue(path[index], out var child):
                AppendPath(child, path, index + 1, context);
                break;
        }
    }

    private static string[] ParsePath(string path)
    {
        // Also support the JSON path syntax, e.g. '$.items[*].label'.
        path = path.Trim().TrimStart('$').Replace("[*]", string.Empty, StringComparison.Ordinal).Replace("[]", string.Empty, StringComparison.Ordinal);

        return path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
