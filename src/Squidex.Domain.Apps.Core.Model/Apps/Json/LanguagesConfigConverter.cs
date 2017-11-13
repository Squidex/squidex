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
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class LanguagesConfigConverter : JsonClassConverter<LanguagesConfig>
    {
        protected override void WriteValue(JsonWriter writer, LanguagesConfig value, JsonSerializer serializer)
        {
            var json = new Dictionary<string, JsonLanguageConfig>(value.Count);

            foreach (LanguageConfig config in value)
            {
                json.Add(config.Language, new JsonLanguageConfig(config));
            }

            serializer.Serialize(writer, json);
        }

        protected override LanguagesConfig ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
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
    }
}
