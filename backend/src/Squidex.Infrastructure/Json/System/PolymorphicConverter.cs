// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Json.System;

public sealed class PolymorphicConverter<T> : JsonConverter<T> where T : class
{
    private readonly JsonEncodedText discriminatorProperty;
    private readonly string discriminatorName;
    private readonly TypeRegistry typeRegistry;

    public PolymorphicConverter(TypeRegistry typeRegistry)
    {
        this.typeRegistry = typeRegistry;

        typeRegistry.TryGetConfig<T>(out var config);

        discriminatorName = config?.DiscriminatorProperty ?? Constants.DefaultDiscriminatorProperty;
        discriminatorProperty = JsonEncodedText.Encode(discriminatorName);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Creating a copy of the reader (The derived deserialisation has to be done from the start)
        Utf8JsonReader typeReader = reader;

        if (typeReader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (typeReader.Read())
        {
            if (typeReader.TokenType == JsonTokenType.PropertyName && IsDiscriminiator(typeReader))
            {
                // Advance the reader to the property value
                typeReader.Read();

                if (typeReader.TokenType != JsonTokenType.String)
                {
                    ThrowHelper.JsonException($"Expected string discriminator value, got '{reader.TokenType}'");
                    return default!;
                }

                // Resolve the type from the discriminator value.
                var type = GetDiscriminatorType(typeReader.GetString()!);

                // Perform the actual deserialization with the original reader
                return (T)JsonSerializer.Deserialize(ref reader, type, options)!;
            }
            else if (typeReader.TokenType == JsonTokenType.StartObject || typeReader.TokenType == JsonTokenType.StartArray)
            {
                if (!typeReader.TrySkip())
                {
                    typeReader.Skip();
                }
            }
        }

        ThrowHelper.JsonException($"Object has no discriminator '{discriminatorName}.");
        return default!;
    }

    private bool IsDiscriminiator(Utf8JsonReader typeReader)
    {
        return
            typeReader.ValueTextEquals(discriminatorProperty.EncodedUtf8Bytes) ||
            typeReader.ValueTextEquals(Constants.DefaultDiscriminatorProperty);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        EnsureTypeResolver(options);

        JsonSerializer.Serialize<object>(writer, value!, options);
    }

    private static void EnsureTypeResolver(JsonSerializerOptions options)
    {
        if (options.TypeInfoResolver is not PolymorphicTypeResolver)
        {
            ThrowHelper.JsonException($"TypeInfoResolver must be of type PolymorphicTypeResolver.");
        }
    }

    private Type GetDiscriminatorType(string name)
    {
        if (!typeRegistry.TryGetType<T>(name, out var type))
        {
            ThrowHelper.JsonException($"Object has invalid discriminator '{name}'.");
            return default!;
        }

        return type;
    }
}
