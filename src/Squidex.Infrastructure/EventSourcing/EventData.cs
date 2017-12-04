// ==========================================================================
//  EventData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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