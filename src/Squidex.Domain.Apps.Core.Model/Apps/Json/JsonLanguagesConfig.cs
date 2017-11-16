// ==========================================================================
//  JsonLanguagesConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class JsonLanguagesConfig
    {
        [JsonProperty]
        public Dictionary<string, JsonLanguageConfig> Languages { get; set; }

        [JsonProperty]
        public Language Master { get; set; }

        public JsonLanguagesConfig()
        {
        }

        public JsonLanguagesConfig(LanguagesConfig value)
        {
            Languages = new Dictionary<string, JsonLanguageConfig>(value.Count);

            foreach (LanguageConfig config in value)
            {
                Languages.Add(config.Language, new JsonLanguageConfig(config));
            }

            Master = value.Master?.Language;
        }

        public LanguagesConfig ToConfig()
        {
            var languagesConfig = new LanguageConfig[Languages?.Count ?? 0];

            if (Languages != null)
            {
                var i = 0;

                foreach (var config in Languages)
                {
                    languagesConfig[i++] = config.Value.ToConfig(config.Key);
                }
            }

            var result = LanguagesConfig.Build(languagesConfig);

            if (Master != null)
            {
                result.MakeMaster(Master);
            }

            return result;
        }
    }
}
