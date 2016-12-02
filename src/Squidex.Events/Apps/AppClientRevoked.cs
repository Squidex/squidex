// ==========================================================================
//  AppClientRevoked.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppClientRevokedEvent")]
    public sealed class AppClientRevoked : IEvent
    {
        public string ClientId { get; set; }
    }
}
