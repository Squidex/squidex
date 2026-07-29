// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

public static class JsonMapper
{
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
                return JsNumber.Create(n);
            case string s:
                return JsString.Create(s);
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

    private static JsObject FromObject(JsonObject obj, Engine engine)
    {
        // Built through the hidden class machinery, so JSON objects sharing a key sequence - every content
        // item of the same schema does - share one hidden class and keep a script reading them monomorphic.
        // A bare ObjectInstance subclass can never be in shape mode and is outside the read caches entirely.
        var entries = new KeyValuePair<string, JsValue>[obj.Count];

        var index = 0;
        foreach (var (key, value) in obj)
        {
            entries[index++] = new KeyValuePair<string, JsValue>(key, Map(value, engine));
        }

        return JsObject.CreateFromEntries(engine, entries);
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

        if (value.IsPromise())
        {
            return Map(value.UnwrapIfPromise());
        }

        if (value is JsArray a)
        {
            var length = a.Length;

            var result = new JsonArray((int)length);

            // The indexed accessor reads the dense backing directly, where a string key would allocate one
            // key per element and route through the full property lookup.
            for (var i = 0u; i < length; i++)
            {
                result.Add(Map(a[i]));
            }

            return result;
        }

        if (value is ObjectWrapper wrapper && wrapper.Target is not IDictionary)
        {
            return JsonValue.Create(wrapper.Target);
        }

        if (value is ObjectInstance obj)
        {
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
