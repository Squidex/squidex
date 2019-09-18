// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;

namespace Squidex.ICIS.Kafka.Entities
{
    public sealed class LocalizedValueText
    {
        [JsonProperty("lang")]
        public string Language { get; set; }

        [JsonProperty("value")]
        public string Text { get; set; }
    }
}
