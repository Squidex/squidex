// ==========================================================================
//  JsonAppPattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using Newtonsoft.Json;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public class JsonAppPattern
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Pattern { get; set; }

        [JsonProperty]
        public string DefaultMessage { get; set; }

        public JsonAppPattern()
        {
        }

        public JsonAppPattern(AppPattern pattern)
        {
            SimpleMapper.Map(pattern, this);
        }

        public AppPattern ToPattern()
        {
            return new AppPattern
            {
                Name = this.Name,
                Pattern = this.Pattern,
                DefaultMessage = this.DefaultMessage
            };
        }
    }
}
