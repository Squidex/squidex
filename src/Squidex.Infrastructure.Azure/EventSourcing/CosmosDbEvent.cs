// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class CosmosDbEvent
    {
        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string Payload { get; set; }

        [JsonProperty]
        public EnvelopeHeaders Headers { get; set; }

        public static CosmosDbEvent FromEventData(EventData data)
        {
            return new CosmosDbEvent { Type = data.Type, Headers = data.Headers, Payload = data.Payload };
        }

        public EventData ToEventData()
        {
            return new EventData(Type, Headers, Payload);
        }
    }
}