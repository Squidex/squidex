// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Squidex.Infrastructure.Reflection;

#pragma warning disable MA0084 // Local variables should not hide other symbols

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

    public static Action<JsonTypeInfo> Modifier(TypeRegistry typeRegistry)
    {
        return new Action<JsonTypeInfo>(typeInfo =>
        {
            var baseType = typeInfo.Type.BaseType;

            while (baseType != null)
            {
                if (typeRegistry.TryGetConfig(baseType, out var config) && config.TryGetName(typeInfo.Type, out var typeName))
                {
                    var discriminatorName = config.DiscriminatorProperty ?? Constants.DefaultDiscriminatorProperty;
                    var discriminatorField = typeInfo.CreateJsonPropertyInfo(typeof(string), discriminatorName);

                    discriminatorField.Get = x =>
                    {
                        return typeName;
                    };

                    typeInfo.Properties.Insert(0, discriminatorField);
                }

                baseType = baseType.BaseType;
            }
        });
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Creating a copy of the reader (The derived deserialisation has to be done from the start)
        Utf8JsonReader typeReader = reader;

        if (typeReader.TokenType != JsonTokenType.StartObject)
        {
            ThrowHelper.JsonSystemException($"Expected Object, got '{reader.TokenType}'");
        }

        while (typeReader.Read())
        {
            if (typeReader.TokenType == JsonTokenType.PropertyName && IsDiscriminiator(typeReader))
            {
                // Advance the reader to the property value
                typeReader.Read();

                if (typeReader.TokenType != JsonTokenType.String)
                {
                    ThrowHelper.JsonSystemException($"Expected string discriminator value, got '{reader.TokenType}'");
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

        ThrowHelper.JsonSystemException($"Object has no discriminator '{discriminatorName}'.");
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
        JsonSerializer.Serialize<object>(writer, value!, options);
    }

    private Type GetDiscriminatorType(string name)
    {
        if (!typeRegistry.TryGetType<T>(name, out var type))
        {
            ThrowHelper.JsonSystemException($"Object has invalid discriminator '{name}'.");
            return default!;
        }

        return type;
    }
}
