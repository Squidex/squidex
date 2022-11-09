// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Json.Objects;

public class JsonObject : Dictionary<string, JsonValue>, IEquatable<JsonObject>
{
    public JsonObject()
    {
    }

    public JsonObject(int capacity)
        : base(capacity)
    {
    }

    public JsonObject(JsonObject source)
        : base(source)
    {
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as JsonObject);
    }

    public bool Equals(JsonObject? other)
    {
        return this.EqualsDictionary(other);
    }

    public override int GetHashCode()
    {
        return this.DictionaryHashCode();
    }

    public override string ToString()
    {
        return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value.ToJsonString()}"))}}}";
    }

    public new JsonObject Add(string key, JsonValue value)
    {
        this[key] = value;

        return this;
    }
}
