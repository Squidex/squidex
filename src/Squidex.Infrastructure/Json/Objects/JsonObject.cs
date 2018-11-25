// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Json.Objects
{
    public sealed class JsonObject : IReadOnlyDictionary<string, IJsonValue>, IJsonValue, IEquatable<JsonObject>
    {
        private readonly Dictionary<string, IJsonValue> inner = new Dictionary<string, IJsonValue>();

        public IJsonValue this[string key]
        {
            get
            {
                return inner[key];
            }
            set
            {
                Guard.NotNullOrEmpty(key, nameof(key));
                Guard.NotNull(value, nameof(value));

                inner[key] = value;
            }
        }

        public IEnumerable<string> Keys
        {
            get { return inner.Keys; }
        }

        public IEnumerable<IJsonValue> Values
        {
            get { return inner.Values; }
        }

        public int Count
        {
            get { return inner.Count; }
        }

        public JsonValueType Type
        {
            get { return JsonValueType.Array; }
        }

        public JsonObject Add(string key, object value)
        {
            return Add(key, JsonValue.Create(value));
        }

        public JsonObject Add(string key, IJsonValue value)
        {
            Guard.NotNullOrEmpty(key, nameof(key));
            Guard.NotNull(value, nameof(value));

            inner.Add(key, value);

            return this;
        }

        public void Clear()
        {
            inner.Clear();
        }

        public bool Remove(string key)
        {
            return inner.Remove(key);
        }

        public bool ContainsKey(string key)
        {
            return inner.ContainsKey(key);
        }

        public bool TryGetValue(string key, out IJsonValue value)
        {
            return inner.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, IJsonValue>> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonObject);
        }

        public bool Equals(IJsonValue other)
        {
            return Equals(other as JsonObject);
        }

        public bool Equals(JsonObject other)
        {
            return other != null && inner.EqualsDictionary(other.inner);
        }

        public override int GetHashCode()
        {
            return inner.DictionaryHashCode();
        }

        public string ToJsonString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return $"{{{string.Join(", ", this.Select(x => $"\"{x.Key}\":{x.Value.ToJsonString()}"))}}}";
        }
    }
}
