// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Json.System
{
    public sealed class InheritanceConverter<T> : JsonConverter<T> where T : notnull
    {
        private readonly JsonEncodedText type = JsonEncodedText.Encode("$type");
        private readonly TypeNameRegistry typeNameRegistry;

        public InheritanceConverter(TypeNameRegistry typeNameRegistry)
        {
            this.typeNameRegistry = typeNameRegistry;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var document = JsonDocument.ParseValue(ref reader);

            if (!document.RootElement.TryGetProperty("$type", out var discriminator))
            {
                ThrowHelper.JsonException("Object has no discriminator '$type'.");
                return default!;
            }

            var typeString = discriminator.GetString()!;
            var typeInfo = typeNameRegistry.GetTypeOrNull(typeString);

            if (typeInfo == null)
            {
                typeInfo = Type.GetType(typeString);
            }

            if (typeInfo == null)
            {
                ThrowHelper.JsonException($"Object has invalid discriminator '{typeString}'.");
                return default!;
            }

            using var bufferWriter = DefaultPools.MemoryStream.GetStream();

            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                document.RootElement.WriteTo(writer);
            }

            return (T?)JsonSerializer.Deserialize(bufferWriter.ToArray(), typeInfo, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var typeName = typeNameRegistry.GetNameOrNull(value.GetType());

            if (typeName == null)
            {
                // Use the type name as a fallback.
                typeName = value.GetType().AssemblyQualifiedName;
            }

            writer.WriteStartObject();
            writer.WriteString(type, typeName);

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
