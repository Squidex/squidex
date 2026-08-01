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
        // Objects that are created this way and have the same keys - all content items of a schema do -
        // share one description of their layout, like a class. Reading a property is then a lot faster than
        // with a custom ObjectInstance class, where every single object gets its own property dictionary.
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

            // The indexer reads the array storage directly. The old version converted the index to a string
            // and did a full property lookup for every element.
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
