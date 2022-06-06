// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class TypeConverterJsonConverter<T> : JsonConverter, ISupportedTypes
    {
        private readonly TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));

        public IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(T); }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return default(T);
            }

            try
            {
                return typeConverter.ConvertFromInvariantString(reader.Value?.ToString()!);
            }
            catch (Exception ex)
            {
                ThrowHelper.JsonException("Error while converting from string.", ex);
                return default;
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(typeConverter.ConvertToInvariantString(value));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }
    }
}
