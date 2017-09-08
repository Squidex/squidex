// ==========================================================================
//  AppContributorRemoved.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppContributorRemoved))]
    public sealed class AppContributorRemoved : AppEvent
    {
        public string ContributorId { get; set; }
    }
}
