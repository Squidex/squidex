// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Json.Objects
{
    public abstract class JsonScalar<T> : IJsonValue, IEquatable<JsonScalar<T>> where T : notnull
    {
        public abstract JsonValueType Type { get; }

        public T Value { get; }

        protected JsonScalar(T value)
        {
            Value = value;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as JsonScalar<T>);
        }

        public bool Equals(IJsonValue? other)
        {
            return Equals(other as JsonScalar<T>);
        }

        public bool Equals(JsonScalar<T>? other)
        {
            return other != null && other.Type == Type && Equals(other.Value, Value);
        }

        public IJsonValue Clone()
        {
            return this;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString()!;
        }

        public virtual string ToJsonString()
        {
            return ToString();
        }

        public bool TryGet(string pathSegment, [MaybeNullWhen(false)] out IJsonValue result)
        {
            result = null!;

            return false;
        }
    }
}
