// ==========================================================================
//  AppClientRevoked.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppClientRevokedEvent")]
    public sealed class AppClientRevoked : AppEvent
    {
        public string Id { get; set; }
    }
}
