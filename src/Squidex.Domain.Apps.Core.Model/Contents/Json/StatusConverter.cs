// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public sealed class StatusConverter : JsonConverter, ISupportedTypes
    {
        public IEnumerable<Type> SupportedTypes
        {
            get { yield return typeof(Status); }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            return new Status(reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Status);
        }
    }
}
