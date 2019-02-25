// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class CosmosDbEventCommit
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public CosmosDbEvent[] Events { get; set; }

        [JsonProperty]
        public long EventStreamOffset { get; set; }

        [JsonProperty]
        public long EventsCount { get; set; }

        [JsonProperty]
        public string EventStream { get; set; }

        [JsonProperty]
        public long Timestamp { get; set; }
    }
}
