// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class DomainIdConverter : JsonConverter, ISupportedTypes
    {
        public IEnumerable<Type> SupportedTypes
        {
            get
            {
                yield return typeof(DomainId);
                yield return typeof(DomainId?);
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.String)
            {
                return DomainId.Create(reader.Value.ToString()!);
            }

            if (reader.TokenType == JsonToken.Null && objectType == typeof(DomainId?))
            {
                return null;
            }

            throw new JsonException($"Not a valid date time, expected String, but got {reader.TokenType}.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DomainId) || objectType == typeof(DomainId?);
        }
    }
}