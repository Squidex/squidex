// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Squidex.Infrastructure.Json.Objects
{
    public sealed class JsonArray : Collection<IJsonValue>, IJsonValue, IEquatable<JsonArray>
    {
        public JsonValueType Type
        {
            get { return JsonValueType.Array; }
        }

        public JsonArray()
        {
        }

        internal JsonArray(params object[] values)
            : base(values?.Select(JsonValue.Create).ToList())
        {
        }

        protected override void InsertItem(int index, IJsonValue item)
        {
            base.InsertItem(index, item ?? JsonValue.Null);
        }

        protected override void SetItem(int index, IJsonValue item)
        {
            base.SetItem(index, item ?? JsonValue.Null);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonArray);
        }

        public bool Equals(IJsonValue other)
        {
            return Equals(other as JsonArray);
        }

        public bool Equals(JsonArray array)
        {
            if (array == null || array.Count != Count)
            {
                return false;
            }

            for (var i = 0; i < Count; i++)
            {
                if (!this[i].Equals(array[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 17;

            for (var i = 0; i < Count; i++)
            {
                hashCode = (hashCode * 23) + this[i].GetHashCode();
            }

            return hashCode;
        }

        public string ToJsonString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", this.Select(x => x.ToJsonString()))}]";
        }
    }
}
