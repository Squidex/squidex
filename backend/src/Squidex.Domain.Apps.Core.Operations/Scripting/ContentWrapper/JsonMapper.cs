// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public static class JsonMapper
    {
        public static JsValue Map(IJsonValue? value, Engine engine)
        {
            if (value == null)
            {
                return JsValue.Null;
            }

            switch (value)
            {
                case JsonNull:
                    return JsValue.Null;
                case JsonString s:
                    return new JsString(s.Value);
                case JsonBoolean b:
                    return new JsBoolean(b.Value);
                case JsonNumber b:
                    return new JsNumber(b.Value);
                case JsonObject obj:
                    return FromObject(obj, engine);
                case JsonArray arr:
                    return FromArray(arr, engine);
            }

            throw new ArgumentException("Invalid json type.", nameof(value));
        }

        private static JsValue FromArray(JsonArray arr, Engine engine)
        {
            var target = new JsValue[arr.Count];

            for (var i = 0; i < arr.Count; i++)
            {
                target[i] = Map(arr[i], engine);
            }

            return engine.Array.Construct(target);
        }

        private static JsValue FromObject(JsonObject obj, Engine engine)
        {
            var target = new ObjectInstance(engine);

            foreach (var (key, value) in obj)
            {
                target.FastAddProperty(key, Map(value, engine), false, true, true);
            }

            target.PreventExtensions();

            return target;
        }

        public static IJsonValue Map(JsValue? value)
        {
            if (value == null || value.IsNull() || value.IsUndefined())
            {
                return JsonValue.Null;
            }

            if (value.IsString())
            {
                return JsonValue.Create(value.AsString());
            }

            if (value.IsBoolean())
            {
                return JsonValue.Create(value.AsBoolean());
            }

            if (value.IsNumber())
            {
                return JsonValue.Create(value.AsNumber());
            }

            if (value.IsDate())
            {
                return JsonValue.Create(value.AsDate().ToString());
            }

            if (value.IsRegExp())
            {
                return JsonValue.Create(value.AsRegExp().Value?.ToString());
            }

            if (value.IsArray())
            {
                var arr = value.AsArray();

                var result = JsonValue.Array();

                for (var i = 0; i < arr.Length; i++)
                {
                    result.Add(Map(arr.Get(i.ToString(CultureInfo.InvariantCulture))));
                }

                return result;
            }

            if (value.IsObject())
            {
                var obj = value.AsObject();

                var result = JsonValue.Object();

                foreach (var (key, propertyDescriptor) in obj.GetOwnProperties())
                {
                    result[key.AsString()] = Map(propertyDescriptor.Value);
                }

                return result;
            }

            throw new ArgumentException("Invalid json type.", nameof(value));
        }
    }
}