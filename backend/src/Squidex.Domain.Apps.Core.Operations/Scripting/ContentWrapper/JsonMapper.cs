// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.ContentWrapper;

public static class JsonMapper
{
    public static JsValue Map(JsonValue value, Engine engine)
    {
        switch (value.Value)
        {
            case null:
                return JsValue.Null;
            case bool b:
                return new JsBoolean(b);
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

    private static JsValue FromArray(JsonArray arr, Engine engine)
    {
        var target = new JsValue[arr.Count];

        for (var i = 0; i < arr.Count; i++)
        {
            target[i] = Map(arr[i], engine);
        }

        return engine.Realm.Intrinsics.Array.Construct(target);
    }

    private static JsValue FromObject(JsonObject obj, Engine engine)
    {
        var target = new ObjectInstance(engine);

        foreach (var (key, value) in obj)
        {
            target.FastAddProperty(key, Map(value, engine), true, true, true);
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

        if (value.IsObject())
        {
            var obj = value.AsObject();

            var result = new JsonObject((int)obj.Length);

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
