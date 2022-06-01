﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.Json.Objects
{
    public class JsonObject : ListDictionary<string, JsonValue2>, IEquatable<JsonObject>
    {
        public JsonObject()
        {
        }

        public JsonObject(int capacity)
            : base(capacity)
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
            return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value}"))}}}";
        }

        public string ToJsonString()
        {
            return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value.ToJsonString()}"))}}}";
        }

        public new JsonObject Add(string key, JsonValue2 value)
        {
            this[key] = value;

            return this;
        }
    }
}
