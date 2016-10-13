// ==========================================================================
//  AppCreated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Apps
{
    public class AppCreated : IEvent
    {
        public string Name { get; set; }
    }
}
