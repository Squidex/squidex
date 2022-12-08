// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public static class ContentConverterFlat
{
    public static Dictionary<string, object?> ToFlatten(this ContentData content)
    {
        var result = new Dictionary<string, object?>();

        foreach (var (key, value) in content)
        {
            var first = GetFirst(value);

            if (first != null)
            {
                result[key] = first;
            }
        }

        return result;
    }

    public static FlatContentData ToFlatten(this ContentData content, string fallback)
    {
        var result = new FlatContentData();

        foreach (var (key, value) in content)
        {
            if (TryGetFirst(value, fallback, out var first))
            {
                result[key] = first;
            }
        }

        return result;
    }

    private static object? GetFirst(ContentFieldData? fieldData)
    {
        if (fieldData == null || fieldData.Count == 0)
        {
            return null;
        }

        if (fieldData.Count == 1)
        {
            return fieldData.Values.First();
        }

        return fieldData;
    }

    private static bool TryGetFirst(ContentFieldData? fieldData, string fallback, out JsonValue result)
    {
        result = JsonValue.Null;

        if (fieldData == null)
        {
            return false;
        }

        if (fieldData.Count == 1)
        {
            result = fieldData.Values.First();
            return true;
        }

        if (fieldData.TryGetValue(fallback, out var value))
        {
            result = value;
            return true;
        }

        if (fieldData.Count > 1)
        {
            result = fieldData.Values.First();
            return true;
        }

        return false;
    }
}
