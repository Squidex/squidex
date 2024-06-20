// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents;

public static class Updates
{
    public static bool IsUnset(object? value)
    {
        if (value is JsonValue json)
        {
            return IsUnset(json.Value);
        }

        return value is IReadOnlyDictionary<string, JsonValue> obj && IsUnset(obj);
    }

    public static bool IsUnset(IReadOnlyDictionary<string, JsonValue>? obj)
    {
        return
            obj is { Count: 1 } &&
            obj.TryGetValue("$unset", out var item) &&
            Equals(item.Value, true);
    }

    public static bool IsUpdate(object? value, out string expression)
    {
        expression = null!;

        if (value is JsonValue json)
        {
            return IsUpdate(json.Value, out expression);
        }

        return value is IReadOnlyDictionary<string, JsonValue> obj && IsUpdate(obj, out expression);
    }

    public static bool IsUpdate(IReadOnlyDictionary<string, JsonValue>? obj, out string expression)
    {
        expression = null!;

        if (obj is { Count: > 0 } &&
            obj.TryGetValue("$update", out var item) &&
            item.Value is string e &&
            !string.IsNullOrWhiteSpace(e))
        {
            expression = e;
            return true;
        }

        return false;
    }
}
