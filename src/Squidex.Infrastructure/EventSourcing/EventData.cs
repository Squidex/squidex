﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class EventData
    {
        public EnvelopeHeaders Headers { get; }

        public string Payload { get; }

        public string Type { get; set; }

        public EventData(string type, EnvelopeHeaders headers, string payload)
        {
            Guard.NotNull(type);
            Guard.NotNull(headers);
            Guard.NotNull(payload);

            Headers = headers;

            Payload = payload;

            Type = type;
        }
    }
}