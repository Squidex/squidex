// ==========================================================================
//  AppClientAttached.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppClientAttachedEvent")]
    public sealed class AppClientAttached : IEvent
    {
        public string ClientName { get; set; }

        public string ClientSecret { get; set; }

        public DateTime ExpiresUtc { get; set; }
    }
}
