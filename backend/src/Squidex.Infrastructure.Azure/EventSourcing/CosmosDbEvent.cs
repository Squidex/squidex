// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class CosmosDbEvent
    {
        public string Type { get; set; }

        public string Payload { get; set; }

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