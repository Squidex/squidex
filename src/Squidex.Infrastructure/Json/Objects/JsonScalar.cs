// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Json.Objects
{
    public sealed class JsonScalar<T> : IJsonValue, IEquatable<JsonScalar<T>>
    {
        private readonly T value;

        public JsonValueType Type { get; }

        public T Value
        {
            get { return value; }
        }

        internal JsonScalar(JsonValueType type, T value)
        {
            Type = type;

            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonScalar<T>);
        }

        public bool Equals(IJsonValue other)
        {
            return Equals(other as JsonScalar<T>);
        }

        public bool Equals(JsonScalar<T> other)
        {
            return other != null && other.Type == Type && Equals(other.value, value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public string ToJsonString()
        {
            return Type == JsonValueType.String ? $"\"{value}\"" : ToString();
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
