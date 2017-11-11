// ==========================================================================
//  EventConsumerRegistryGrainState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Infrastructure.CQRS.Events.Grains.Implementation
{
    public sealed class EventConsumerRegistryGrainState
    {
        public HashSet<string> EventConsumerNames { get; set; }

        public EventConsumerRegistryGrainState()
        {
            EventConsumerNames = new HashSet<string>();
        }
    }
}
