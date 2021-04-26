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
    public abstract class JsonClassConverter<T> : JsonConverter, ISupportedTypes where T : class
    {
        public virtual IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(T); }
        }

        public sealed override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            return ReadValue(reader, objectType, serializer);
        }

        protected abstract T? ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer);

        public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            WriteValue(writer, (T)value, serializer);
        }

        protected abstract void WriteValue(JsonWriter writer, T value, JsonSerializer serializer);

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }
    }
}
