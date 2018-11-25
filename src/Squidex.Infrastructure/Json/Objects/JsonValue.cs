// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Json.Objects
{
    public static class JsonValue
    {
        public static readonly JsonScalar<string> Empty = new JsonScalar<string>(JsonValueType.String, string.Empty);

        public static readonly JsonScalar<bool> True = new JsonScalar<bool>(JsonValueType.Boolean, true);
        public static readonly JsonScalar<bool> False = new JsonScalar<bool>(JsonValueType.Boolean, false);

        public static readonly JsonNull Null = JsonNull.Null;

        public static JsonArray Array()
        {
            return new JsonArray();
        }

        public static JsonArray Array(params object[] values)
        {
            return new JsonArray(values);
        }

        public static JsonObject Object()
        {
            return new JsonObject();
        }

        public static IJsonValue Create(object value)
        {
            if (value == null)
            {
                return Null;
            }

            if (value is IJsonValue v)
            {
                return v;
            }

            switch (value)
            {
                case string s:
                    return Create(s);
                case bool b:
                    return Create(b);
                case float f:
                    return Create(f);
                case double d:
                    return Create(d);
                case int i:
                    return Create(i);
                case long l:
                    return Create(l);
            }

            throw new ArgumentException("Invalid json type");
        }

        public static IJsonValue Create(bool value)
        {
            return value ? True : False;
        }

        public static IJsonValue Create(double value)
        {
            Guard.ValidNumber(value, nameof(value));

            return new JsonScalar<double>(JsonValueType.Number, value);
        }

        public static IJsonValue Create(double? value)
        {
            if (value == null)
            {
                return Null;
            }

            return Create(value.Value);
        }

        public static IJsonValue Create(bool? value)
        {
            if (value == null)
            {
                return Null;
            }

            return Create(value.Value);
        }

        public static IJsonValue Create(string value)
        {
            if (value == null)
            {
                return Null;
            }

            if (value.Length == 0)
            {
                return Empty;
            }

            return new JsonScalar<string>(JsonValueType.String, value);
        }
    }
}
