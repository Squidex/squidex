// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json;
using JsonException = Squidex.Infrastructure.Json.JsonException;

namespace Squidex.Infrastructure.Queries.Json
{
    public sealed class CompareOperatorJsonConverter : JsonConverter, ISupportedTypes
    {
        private readonly TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(CompareOperator));

        public IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(CompareOperator); }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                return typeConverter.ConvertFromInvariantString(reader.Value?.ToString()!);
            }
            catch (InvalidCastException ex)
            {
                throw new JsonException(ex.Message);
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(typeConverter.ConvertToInvariantString(value));
        }

        public override bool CanConvert(Type objectType)
        {
            return SupportedTypes.Contains(objectType);
        }
    }
}
