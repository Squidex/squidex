// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Squidex.Infrastructure.Json.System;

public abstract class InheritanceConverterBase<T> : JsonConverter<T>, IInheritanceConverter where T : notnull
{
    private readonly JsonEncodedText discriminatorProperty;

    public string DiscriminatorName { get; }

    protected InheritanceConverterBase(string discriminatorName)
    {
        discriminatorProperty = JsonEncodedText.Encode(discriminatorName);
        DiscriminatorName = discriminatorName;
    }

    public abstract Type GetDiscriminatorType(string name, Type typeToConvert);

    public abstract string GetDiscriminatorValue(Type type);

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
            if (typeReader.TokenType == JsonTokenType.PropertyName &&
                typeReader.ValueTextEquals(discriminatorProperty.EncodedUtf8Bytes))
            {
                // Advance the reader to the property value
                typeReader.Read();

                if (typeReader.TokenType != JsonTokenType.String)
                {
                    ThrowHelper.JsonException($"Expected string discriminator value, got '{reader.TokenType}'");
                    return default!;
                }

                // Resolve the type from the discriminator value.
                var type = GetDiscriminatorType(typeReader.GetString()!, typeToConvert);

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

        ThrowHelper.JsonException($"Object has no discriminator '{DiscriminatorName}.");
        return default!;
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
}
