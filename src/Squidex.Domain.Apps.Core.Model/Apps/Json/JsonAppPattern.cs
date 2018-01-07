// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
        public string Message { get; set; }

        public JsonAppPattern()
        {
        }

        public JsonAppPattern(AppPattern pattern)
        {
            SimpleMapper.Map(pattern, this);
        }

        public AppPattern ToPattern()
        {
            return new AppPattern(Name, Pattern, Message);
        }
    }
}
