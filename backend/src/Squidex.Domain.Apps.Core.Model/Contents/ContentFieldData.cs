// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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

        public ContentFieldData AddValue(object? value)
        {
            return AddJsonValue(JsonValue.Create(value));
        }

        public ContentFieldData AddValue(string key, object? value)
        {
            return AddJsonValue(key, JsonValue.Create(value));
        }

        public ContentFieldData AddJsonValue(IJsonValue value)
        {
            this[InvariantPartitioning.Key] = value;

            return this;
        }

        public ContentFieldData AddJsonValue(string key, IJsonValue value)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            if (Language.IsValidLanguage(key))
            {
                this[key] = value;
            }
            else
            {
                this[key] = value;
            }

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