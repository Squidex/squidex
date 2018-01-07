// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public class JsonLanguageConfig
    {
        [JsonProperty]
        public Language[] Fallback { get; set; }

        [JsonProperty]
        public bool IsOptional { get; set; }

        public JsonLanguageConfig()
        {
        }

        public JsonLanguageConfig(LanguageConfig config)
        {
            SimpleMapper.Map(config, this);

            Fallback = config.LanguageFallbacks.ToArray();
        }

        public LanguageConfig ToConfig(string language)
        {
            return new LanguageConfig(language, IsOptional, Fallback);
        }
    }
}
