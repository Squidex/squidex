// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.Json.System;

public sealed class ReadonlyDictionaryConverterFactory : JsonConverterFactory
{
    private sealed class Converter<TKey, TValue, TInstance> : JsonConverter<TInstance> where TKey : notnull
    {
        private readonly Type innerType = typeof(IReadOnlyDictionary<TKey, TValue>);
        private readonly Func<IDictionary<TKey, TValue>, TInstance> creator;

        public Converter()
        {
            creator = ReflectionHelper.CreateParameterizedConstructor<TInstance, IDictionary<TKey, TValue>>();
        }

        public override TInstance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var inner = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options)!;

            return creator(inner);
        }

        public override void Write(Utf8JsonWriter writer, TInstance value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, innerType, options);
        }
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return IsReadonlyDictionary(typeToConvert) || IsReadonlyDictionary(typeToConvert.BaseType);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeToCreate = IsReadonlyDictionary(typeToConvert) ? typeToConvert : typeToConvert.BaseType!;

        var concreteType = typeof(Converter<,,>).MakeGenericType(
            new Type[]
            {
                typeToCreate.GetGenericArguments()[0],
                typeToCreate.GetGenericArguments()[1],
                typeToConvert
            });

        var converter = (JsonConverter)Activator.CreateInstance(concreteType)!;

        return converter;
    }

    private static bool IsReadonlyDictionary(Type? type)
    {
        return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReadonlyDictionary<,>);
    }
}
