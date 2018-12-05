// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using NodaTime;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Squidex.Infrastructure.Json.Objects
{
    public static class JsonValue
    {
        public static readonly IJsonValue Empty = new JsonString(string.Empty);

        public static readonly IJsonValue True = JsonBoolean.True;
        public static readonly IJsonValue False = JsonBoolean.False;

        public static readonly IJsonValue Null = JsonNull.Null;

        public static readonly IJsonValue Zero = new JsonNumber(0);

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
                case Instant i:
                    return Create(i);
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

            if (value == 0)
            {
                return Zero;
            }

            return new JsonNumber(value);
        }

        public static IJsonValue Create(Instant? value)
        {
            if (value == null)
            {
                return Null;
            }

            return Create(value.Value.ToString());
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

            return new JsonString(value);
        }
    }
}
