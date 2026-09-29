// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

public sealed class SearchPathsFieldTextStrategy : IFieldTextStrategy
{
    // Search paths are configured explicitly and therefore override all other strategies.
    public int Order => -1000;

    public bool TryExtract(IField? field, JsonValue value, TextCollector collector)
    {
        if (field?.RawProperties.SearchPaths is not { Count: > 0 } paths)
        {
            return false;
        }

        // Parse the paths only once for all contents of the schema.
        var parsedPaths =
            collector.Context?.GetOrAdd((typeof(SearchPathsFieldTextStrategy), paths), () => ParsePaths(paths)) ??
            ParsePaths(paths);

        foreach (var path in parsedPaths)
        {
            AppendPath(value, path, 0, collector);
        }

        return true;
    }

    private static void AppendPath(JsonValue value, string[] path, int index, TextCollector collector)
    {
        if (index == path.Length)
        {
            collector.AppendValue(value);
            return;
        }

        switch (value.Value)
        {
            case JsonArray array:
                // Arrays are traversed implicitly, so that users do not have to specify the index.
                foreach (var item in array)
                {
                    AppendPath(item, path, index, collector);
                }

                break;
            case JsonObject obj when obj.TryGetValue(path[index], out var child):
                AppendPath(child, path, index + 1, collector);
                break;
        }
    }

    private static string[][] ParsePaths(ReadonlyList<string> paths)
    {
        return paths.Select(ParsePath).ToArray();
    }

    private static string[] ParsePath(string path)
    {
        // Also support the JSON path syntax, e.g. '$.items[*].label'.
        path = path.Trim().TrimStart('$').Replace("[*]", string.Empty, StringComparison.Ordinal).Replace("[]", string.Empty, StringComparison.Ordinal);

        return path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
