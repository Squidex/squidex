// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Squidex.Infrastructure.Json.Objects;

public sealed class JsonArray : List<JsonValue>, IEquatable<JsonArray>
{
    public JsonArray()
    {
    }

    public JsonArray(int capacity)
        : base(capacity)
    {
    }

    public JsonArray(JsonArray source)
        : base(source)
    {
    }

    public JsonArray(IEnumerable<JsonValue>? source)
    {
        if (source != null)
        {
            foreach (var item in source)
            {
                Add(item);
            }
        }
    }

    public new JsonArray Add(JsonValue value)
    {
        base.Add(value);

        return this;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as JsonArray);
    }

    public bool Equals(JsonArray? array)
    {
        return this.EqualsList(array);
    }

    public override int GetHashCode()
    {
        return this.SequentialHashCode();
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", this.Select(x => x.ToJsonString()))}]";
    }

    public bool TryGetValue(string pathSegment, [MaybeNullWhen(false)] out JsonValue result)
    {
        Guard.NotNull(pathSegment);

        result = default;

        if (pathSegment != null && int.TryParse(pathSegment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) && index >= 0 && index < Count)
        {
            result = this[index];

            return true;
        }

        return false;
    }
}
