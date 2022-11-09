// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents;

public sealed class ContentFieldData : Dictionary<string, JsonValue>, IEquatable<ContentFieldData>
{
    public ContentFieldData()
        : base(0, StringComparer.OrdinalIgnoreCase)
    {
    }

    public ContentFieldData(int capacity)
        : base(capacity, StringComparer.OrdinalIgnoreCase)
    {
    }

    public ContentFieldData(ContentFieldData source)
        : base(source.Count, StringComparer.OrdinalIgnoreCase)
    {
        foreach (var (key, value) in source)
        {
            this[key] = value;
        }
    }

    public bool TryGetNonNull(string key, [MaybeNullWhen(false)] out JsonValue result)
    {
        result = JsonValue.Null;

        if (TryGetValue(key, out var found) && found != default)
        {
            result = found;
            return true;
        }

        return false;
    }

    public ContentFieldData AddInvariant(JsonValue value)
    {
        this[InvariantPartitioning.Key] = value;

        return this;
    }

    public ContentFieldData AddLocalized(string key, JsonValue value)
    {
        this[key] = value;

        return this;
    }

    public ContentFieldData Clone()
    {
        var clone = new ContentFieldData(Count);

        foreach (var (key, value) in this)
        {
            clone[key] = value.Clone()!;
        }

        return clone;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ContentFieldData);
    }

    public bool Equals(ContentFieldData? other)
    {
        return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
    }

    public override int GetHashCode()
    {
        return this.DictionaryHashCode();
    }

    public override string ToString()
    {
        return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value}"))}}}";
    }
}
