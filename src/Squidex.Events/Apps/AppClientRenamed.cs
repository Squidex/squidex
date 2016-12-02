// ==========================================================================
//  AppClientRenamed.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppClientRenamedEvent")]
    public sealed class AppClientRenamed : IEvent
    {
        public string ClientId { get; set; }

        public string Name { get; set; }
    }
}
