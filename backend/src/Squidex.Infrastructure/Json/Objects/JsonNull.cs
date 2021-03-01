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
    public sealed class JsonNull : IJsonValue, IEquatable<JsonNull>
    {
        public static readonly JsonNull Null = new JsonNull();

        public JsonValueType Type
        {
            get => JsonValueType.Null;
        }

        private JsonNull()
        {
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as JsonNull);
        }

        public bool Equals(IJsonValue? other)
        {
            return Equals(other as JsonNull);
        }

        public bool Equals(JsonNull? other)
        {
            return other != null;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public IJsonValue Clone()
        {
            return this;
        }

        public string ToJsonString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "null";
        }

        public bool TryGet(string pathSegment, [MaybeNullWhen(false)] out IJsonValue result)
        {
            result = null!;

            return false;
        }
    }
}
