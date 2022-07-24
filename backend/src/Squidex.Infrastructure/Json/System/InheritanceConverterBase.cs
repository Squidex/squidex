// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Infrastructure.Json.System
{
    public abstract class InheritanceConverterBase<T> : JsonConverter<T> where T : notnull
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

            if (!typeReader.Read() || typeReader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = typeReader.GetString();

            if (typeReader.Read() && typeReader.TokenType == JsonTokenType.String && propertyName == DiscriminatorName)
            {
                var type = GetDiscriminatorType(typeReader.GetString()!, typeToConvert);

                return (T?)JsonSerializer.Deserialize(ref reader, type, options);
            }
            else
            {
                using var document = JsonDocument.ParseValue(ref reader);

                if (!document.RootElement.TryGetProperty(DiscriminatorName, out var discriminator))
                {
                    ThrowHelper.JsonException($"Object has no discriminator '{DiscriminatorName}.");
                    return default!;
                }

                var type = GetDiscriminatorType(discriminator.GetString()!, typeToConvert);

                using var bufferWriter = DefaultPools.MemoryStream.GetStream();

                using (var writer = new Utf8JsonWriter(bufferWriter))
                {
                    document.RootElement.WriteTo(writer);
                }

                return (T?)JsonSerializer.Deserialize(bufferWriter.ToArray(), type, options);
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var name = GetDiscriminatorValue(value.GetType());

            writer.WriteStartObject();
            writer.WriteString(discriminatorProperty, name);

            using (var document = JsonSerializer.SerializeToDocument(value, value.GetType(), options))
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    property.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }
    }
}
