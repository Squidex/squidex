// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class JsonLanguagesConfig
    {
        [JsonProperty]
        public Dictionary<string, JsonLanguageConfig> Languages { get; set; }

        [JsonProperty]
        public string Master { get; set; }

        public JsonLanguagesConfig()
        {
        }

        public JsonLanguagesConfig(LanguagesConfig value)
        {
            Languages = value.Languages.ToDictionary(x => x.Key, x => new JsonLanguageConfig(x.Value));

            Master = value.Master;
        }

        public LanguagesConfig ToConfig()
        {
            var languages = Languages.ToDictionary(x => x.Key, x => x.Value.ToConfig());

            var master = Master ?? languages.Keys.First();

            return new LanguagesConfig(languages, master);
        }
    }
}
