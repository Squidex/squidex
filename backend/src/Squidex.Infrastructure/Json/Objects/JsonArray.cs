﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

        internal JsonArray(params object?[] values)
            : base(ToList(values))
        {
        }

        private static List<IJsonValue> ToList(IEnumerable<object?> values)
        {
            return values?.Select(JsonValue.Create).ToList() ?? new List<IJsonValue>();
        }

        protected override void InsertItem(int index, IJsonValue item)
        {
            base.InsertItem(index, item ?? JsonValue.Null);
        }

        protected override void SetItem(int index, IJsonValue item)
        {
            base.SetItem(index, item ?? JsonValue.Null);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as JsonArray);
        }

        public bool Equals(IJsonValue? other)
        {
            return Equals(other as JsonArray);
        }

        public bool Equals(JsonArray? array)
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

        public bool TryGet(string pathSegment, [MaybeNullWhen(false)] out IJsonValue result)
        {
            Guard.NotNull(pathSegment);

            if (pathSegment != null && int.TryParse(pathSegment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) && index >= 0 && index < Count)
            {
                result = this[index];

                return true;
            }

            result = null!;

            return false;
        }
    }
}
