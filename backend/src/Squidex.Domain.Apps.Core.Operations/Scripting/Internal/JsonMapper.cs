// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Globalization;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

public static class JsonMapper
{
    private sealed class JsonObjectInstance : ObjectInstance
    {
        public JsonObjectInstance(Engine engine)
            : base(engine)
        {
        }
    }

    public static JsValue Map(JsonValue value, Engine engine)
    {
        switch (value.Value)
        {
            case null:
                return JsValue.Null;
            case true:
                return JsBoolean.True;
            case false:
                return JsBoolean.False;
            case double n:
                return new JsNumber(n);
            case string s:
                return new JsString(s);
            case JsonObject o:
                return FromObject(o, engine);
            case JsonArray a:
                return FromArray(a, engine);
        }

        ThrowInvalidType(nameof(value));
        return JsValue.Null;
    }

    private static JsArray FromArray(JsonArray arr, Engine engine)
    {
        var target = new JsValue[arr.Count];

        for (var i = 0; i < arr.Count; i++)
        {
            target[i] = Map(arr[i], engine);
        }

        return engine.Intrinsics.Array.Construct(target);
    }

    private static JsonObjectInstance FromObject(JsonObject obj, Engine engine)
    {
        var target = new JsonObjectInstance(engine);

        foreach (var (key, value) in obj)
        {
            target.Set(key, Map(value, engine));
        }

        return target;
    }

    public static JsonValue Map(JsValue? value)
    {
        if (value == null || value.IsNull() || value.IsUndefined())
        {
            return default;
        }

        if (value.IsString())
        {
            return value.AsString();
        }

        if (value.IsBoolean())
        {
            return value.AsBoolean();
        }

        if (value.IsDate())
        {
            return value.AsDate().ToString();
        }

        if (value.IsRegExp())
        {
            return value.AsRegExp().Value?.ToString();
        }

        if (value.IsNumber())
        {
            var number = value.AsNumber();

            if (double.IsNaN(number) || double.IsPositiveInfinity(number) || double.IsNegativeInfinity(number))
            {
                return 0;
            }

            return number;
        }

        if (value.IsArray())
        {
            var arr = value.AsArray();

            var result = new JsonArray((int)arr.Length);

            for (var i = 0; i < arr.Length; i++)
            {
                result.Add(Map(arr.Get(i.ToString(CultureInfo.InvariantCulture))));
            }

            return result;
        }

        if (value is ObjectWrapper wrapper && wrapper.Target is not IDictionary)
        {
            return JsonValue.Create(wrapper.Target);
        }

        if (value.IsObject())
        {
            var obj = value.AsObject();

            var result = new JsonObject();

            foreach (var (key, propertyDescriptor) in obj.GetOwnProperties())
            {
                result[key.AsString()] = Map(propertyDescriptor.Value);
            }

            return result;
        }

        ThrowInvalidType(nameof(value));
        return default;
    }

    private static void ThrowInvalidType(string argument)
    {
        ThrowHelper.ArgumentException("Invalid json type.", argument);
    }
}
