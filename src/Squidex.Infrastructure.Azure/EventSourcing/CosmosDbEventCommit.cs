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
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("events")]
        public CosmosDbEvent[] Events { get; set; }

        [JsonProperty("eventStreamOffset")]
        public long EventStreamOffset { get; set; }

        [JsonProperty("eventsCount")]
        public long EventsCount { get; set; }

        [JsonProperty("eventStream")]
        public string EventStream { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }
}
