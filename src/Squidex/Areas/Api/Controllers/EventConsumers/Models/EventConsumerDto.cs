// ==========================================================================
//  EventConsumerDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.EventConsumers.Models
{
    public sealed class EventConsumerDto
    {
        public bool IsStopped { get; set; }

        public bool IsResetting { get; set; }

        public string Name { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }
    }
}
