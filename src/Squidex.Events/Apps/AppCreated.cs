// ==========================================================================
//  AppCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppCreatedEvent")]
    public class AppCreated : IEvent
    {
        public string Name { get; set; }
    }
}
