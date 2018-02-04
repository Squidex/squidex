// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema.Annotations;

#pragma warning disable SA1306 // Field names must begin with lower-case letter

namespace Squidex.Areas.Api.Controllers
{
    public sealed class JsonInheritanceConverter : JsonConverter
    {
        private readonly string discriminator;
        private readonly Dictionary<string, Type> mapNameToType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Type, string> mapTypeToName = new Dictionary<Type, string>();

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

        public JsonInheritanceConverter(string discriminator, Type baseType)
        {
            this.discriminator = discriminator;

            foreach (var type in baseType.Assembly.GetTypes().Where(x => x != baseType && baseType.IsAssignableFrom(x)))
            {
                var name = type.GetTypeInfo().GetCustomAttribute<JsonSchemaAttribute>()?.Name ?? type.Name;

                mapTypeToName[type] = name;
                mapNameToType[name] = type;
            }
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

                jsonObject.AddFirst(new JProperty(discriminator, mapTypeToName[value.GetType()]));

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

                if (subName == null || !mapNameToType.TryGetValue(subName, out var subType))
                {
                    throw new InvalidOperationException($"Could not find subtype of '{objectType.Name}' with discriminator '{subName}'.");
                }

                return serializer.Deserialize(jsonObject.CreateReader(), subType);
            }
            finally
            {
                IsReading = false;
            }
        }
    }
}