// ==========================================================================
//  AppCreated.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.CQRS.Events;

namespace PinkParrot.Events.Apps
{
    [TypeName("AppCreated")]
    public class AppCreated : IEvent
    {
        public string Name { get; set; }
    }
}
