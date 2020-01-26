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
        public Language[]? Fallback { get; set; }

        [JsonProperty]
        public bool IsOptional { get; set; }

        public JsonLanguageConfig()
        {
        }

        public JsonLanguageConfig(LanguageConfig config)
        {
            SimpleMapper.Map(config, this);

            Fallback = config.Fallbacks.ToArray();
        }

        public LanguageConfig ToConfig()
        {
            if (!IsOptional && (Fallback == null || Fallback.Length == 0))
            {
                return LanguageConfig.Default;
            }
            else
            {
                return new LanguageConfig(IsOptional, Fallback);
            }
        }
    }
}
