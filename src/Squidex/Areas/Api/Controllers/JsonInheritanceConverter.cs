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
    public class JsonInheritanceConverter : JsonConverter
    {
        private readonly string discriminator;
        private readonly Dictionary<string, Type> mapTypeToName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Type, string> mapNameToType = new Dictionary<Type, string>();

        [ThreadStatic]
        private static bool IsReading;

        [ThreadStatic]
        private static bool IsWriting;

        public string DiscriminatorName
        {
            get { return discriminator; }
        }

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
            : this(discriminator, baseType, null)
        {
        }

        protected JsonInheritanceConverter(string discriminator, Type baseType,  IReadOnlyDictionary<string, Type> subTypes = null)
        {
            this.discriminator = discriminator;

            if (subTypes != null)
            {
                foreach (var subType in subTypes)
                {
                    mapNameToType[subType.Value] = subType.Key;
                    mapTypeToName[subType.Key] = subType.Value;
                }
            }
            else
            {
                foreach (var type in baseType.Assembly.GetTypes().Where(x => x != baseType && baseType.IsAssignableFrom(x)))
                {
                    var name = type.GetTypeInfo().GetCustomAttribute<JsonSchemaAttribute>()?.Name ?? type.Name;

                    mapNameToType[type] = name;
                    mapTypeToName[name] = type;
                }
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

                jsonObject.AddFirst(new JProperty(discriminator, mapNameToType[value.GetType()]));

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

                if (!mapTypeToName.TryGetValue(subName, out var subType))
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