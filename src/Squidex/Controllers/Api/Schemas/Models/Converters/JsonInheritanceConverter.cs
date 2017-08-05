// ==========================================================================
//  JsonInheritanceConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema.Annotations;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Squidex.Controllers.Api.Schemas.Models.Converters
{
    public sealed class JsonInheritanceConverter : JsonConverter
    {
        private readonly string discriminator;

        [ThreadStatic]
        private static bool IsReading;

        [ThreadStatic]
        private static bool IsWriting;

        public override bool CanWrite
        {
            get
            {
                if (!IsWriting)
                {
                    return true;
                }

                return IsWriting = false;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (!IsReading)
                {
                    return true;
                }

                return IsReading = false;
            }
        }

        public JsonInheritanceConverter(string discriminator)
        {
            this.discriminator = discriminator;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IsWriting = true;
            try
            {
                var jsonObject = JObject.FromObject(value, serializer);

                jsonObject.AddFirst(new JProperty(discriminator, GetSchemaName(value.GetType())));

                writer.WriteToken(jsonObject.CreateReader());
            }
            finally
            {
                IsWriting = false;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IsReading = true;
            try
            {
                var jsonObject = serializer.Deserialize<JObject>(reader);

                var subName = jsonObject[discriminator]?.Value<string>();

                if (subName == null)
                {
                    return null;
                }

                var subType = GetObjectSubtype(objectType, subName);

                if (subType == null)
                {
                    return null;
                }

                return serializer.Deserialize(jsonObject.CreateReader(), subType);
            }
            finally
            {
                IsReading = false;
            }
        }

        private static Type GetObjectSubtype(Type objectType, string discriminatorValue)
        {
            var knownTypeAttribute =
                objectType.GetTypeInfo().GetCustomAttributes<KnownTypeAttribute>()
                    .FirstOrDefault(a => IsKnownType(a, discriminatorValue));

            return knownTypeAttribute?.Type;
        }

        private static bool IsKnownType(KnownTypeAttribute attribute, string discriminator)
        {
            var type = attribute.Type;

            return type != null && GetSchemaName(type) == discriminator;
        }

        private static string GetSchemaName(Type type)
        {
            var schenaName = type.GetTypeInfo().GetCustomAttribute<JsonSchemaAttribute>()?.Name;

            return schenaName ?? type.Name;
        }
    }
}