// ==========================================================================
//  AppClientKeyRevoked.cs
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
    [TypeName("AppClientKeyRevoked")]
    public sealed class AppClientKeyRevoked : IEvent
    {
        public string ClientKey { get; set; }

        public DateTime ExpiresUtc { get; set; }
    }
}
