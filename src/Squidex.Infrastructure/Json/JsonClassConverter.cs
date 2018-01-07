// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public abstract class JsonClassConverter<T> : JsonConverter where T : class
    {
        public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            return ReadValue(reader, objectType, serializer);
        }

        public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            WriteValue(writer, (T)value, serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        protected abstract void WriteValue(JsonWriter writer, T value, JsonSerializer serializer);

        protected abstract T ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer);
    }
}
