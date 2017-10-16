// ==========================================================================
//  JsonMapper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper
{
    public static class JsonMapper
    {
        public static JsValue Map(JToken value, Engine engine)
        {
            if (value == null)
            {
                return JsValue.Null;
            }

            switch (value.Type)
            {
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.String:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    return new JsValue((string)value);
                case JTokenType.Null:
                    return JsValue.Null;
                case JTokenType.Undefined:
                    return JsValue.Undefined;
                case JTokenType.Integer:
                    return new JsValue((long)value);
                case JTokenType.Float:
                    return new JsValue((double)value);
                case JTokenType.Boolean:
                    return new JsValue((bool)value);
                case JTokenType.Object:
                    return FromObject(value, engine);
                case JTokenType.Array:
                {
                    var arr = (JArray)value;

                    var target = new JsValue[arr.Count];

                    for (var i = 0; i < arr.Count; i++)
                    {
                        target[i] = Map(arr[i], engine);
                    }

                    return engine.Array.Construct(target);
                }
            }

            throw new ArgumentException("Invalid json type.", nameof(value));
        }

        private static JsValue FromObject(JToken value, Engine engine)
        {
            var obj = (JObject)value;

            var target = new ObjectInstance(engine);

            foreach (var property in obj)
            {
                target.FastAddProperty(property.Key, Map(property.Value, engine), false, true, true);
            }

            return target;
        }

        public static JToken Map(JsValue value)
        {
            if (value == null || value.IsNull())
            {
                return JValue.CreateNull();
            }

            if (value.IsUndefined())
            {
                return JValue.CreateUndefined();
            }

            if (value.IsString())
            {
                return new JValue(value.AsString());
            }

            if (value.IsBoolean())
            {
                return new JValue(value.AsBoolean());
            }

            if (value.IsNumber())
            {
                return new JValue(value.AsNumber());
            }

            if (value.IsDate())
            {
                return new JValue(value.AsDate().ToDateTime());
            }

            if (value.IsRegExp())
            {
                return JValue.CreateString(value.AsRegExp().Value?.ToString());
            }

            if (value.IsArray())
            {
                var arr = value.AsArray();

                var target = new JArray();

                for (var i = 0; i < arr.GetLength(); i++)
                {
                    target.Add(Map(arr.Get(i.ToString())));
                }

                return target;
            }

            if (value.IsObject())
            {
                var obj = value.AsObject();

                var target = new JObject();

                foreach (var kvp in obj.GetOwnProperties())
                {
                    target[kvp.Key] = Map(kvp.Value.Value);
                }

                return target;
            }

            throw new ArgumentException("Invalid json type.", nameof(value));
        }
    }
}