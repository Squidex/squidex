// ==========================================================================
//  AppClientsConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class LanguagesConfigConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var languagesConfig = (LanguagesConfig)value;

            var json = new Dictionary<string, JsonLanguageConfig>(languagesConfig.Count);

            foreach (var config in languagesConfig.Configs)
            {
                json.Add(config.Language, new JsonLanguageConfig(config));
            }

            serializer.Serialize(writer, json);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, JsonLanguageConfig>>(reader);

            var languagesConfig = new LanguageConfig[json.Count];

            var i = 0;

            foreach (var config in json)
            {
                languagesConfig[i++] = config.Value.ToConfig(config.Key);
            }

            return LanguagesConfig.Build(languagesConfig);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LanguagesConfig);
        }
    }
}
