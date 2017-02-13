// ==========================================================================
//  EventConsumerDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.EventConsumers.Models
{
    public sealed class EventConsumerDto
    {
        public long LastHandledEventNumber { get; set; }

        public bool IsStopped { get; set; }

        public bool IsResetting { get; set; }

        public string Name { get; set; }
    }
}
