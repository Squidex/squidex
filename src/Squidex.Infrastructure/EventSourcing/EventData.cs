// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EventData
    {
        public Guid EventId { get; set; }

        public string Payload { get; set; }

        public string Metadata { get; set; }

        public string Type { get; set; }
    }
}