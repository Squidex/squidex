// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class ContentFieldData : Dictionary<string, IJsonValue>, IEquatable<ContentFieldData>
    {
        public ContentFieldData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentFieldData(ContentFieldData source)
            : base(source, StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentFieldData(int capacity)
            : base(capacity, StringComparer.OrdinalIgnoreCase)
        {
        }

        public bool TryGetNonNull(string key, [MaybeNullWhen(false)] out IJsonValue result)
        {
            result = null!;

            if (TryGetValue(key, out var found) && found != null && found.Type != JsonValueType.Null)
            {
                result = found;
                return true;
            }

            return false;
        }

        public ContentFieldData AddInvariant(object? value)
        {
            return AddValue(InvariantPartitioning.Key, JsonValue.Create(value));
        }

        public ContentFieldData AddLocalized(string key, object? value)
        {
            return AddValue(key, JsonValue.Create(value));
        }

        public ContentFieldData AddValue(string key, IJsonValue? value)
        {
            this[key] = JsonValue.Create(value);

            return this;
        }

        public ContentFieldData Clone()
        {
            var clone = new ContentFieldData(Count);

            foreach (var (key, value) in this)
            {
                clone[key] = value?.Clone()!;
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
            return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value.ToJsonString()}"))}}}";
        }
    }
}