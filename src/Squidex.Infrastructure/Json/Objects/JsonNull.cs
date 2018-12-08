// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Json.Objects
{
    public sealed class JsonNull : IJsonValue, IEquatable<JsonNull>
    {
        public static readonly JsonNull Null = new JsonNull();

        public JsonValueType Type
        {
            get { return JsonValueType.Null; }
        }

        private JsonNull()
        {
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonNull);
        }

        public bool Equals(IJsonValue other)
        {
            return Equals(other as JsonNull);
        }

        public bool Equals(JsonNull other)
        {
            return other != null;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public string ToJsonString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
