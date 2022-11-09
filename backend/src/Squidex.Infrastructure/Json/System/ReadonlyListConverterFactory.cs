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

public sealed class ReadonlyListConverterFactory : JsonConverterFactory
{
    private sealed class Converter<T, TInstance> : JsonConverter<TInstance>
    {
        private readonly Type innerType = typeof(IReadOnlyList<T>);
        private readonly Func<IList<T>, TInstance> creator;

        public Converter()
        {
            creator = ReflectionHelper.CreateParameterizedConstructor<TInstance, IList<T>>();
        }

        public override TInstance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var inner = JsonSerializer.Deserialize<List<T>>(ref reader, options)!;

            return creator(inner);
        }

        public override void Write(Utf8JsonWriter writer, TInstance value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, innerType, options);
        }
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return IsReadonlyList(typeToConvert) || IsReadonlyList(typeToConvert.BaseType);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeToCreate = IsReadonlyList(typeToConvert) ? typeToConvert : typeToConvert.BaseType!;

        var concreteType = typeof(Converter<,>).MakeGenericType(
            new Type[]
            {
                typeToCreate.GetGenericArguments()[0],
                typeToConvert,
            });

        var converter = (JsonConverter)Activator.CreateInstance(concreteType)!;

        return converter;
    }

    private static bool IsReadonlyList(Type? type)
    {
        return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReadonlyList<>);
    }
}
