// ==========================================================================
//  AppContributorAssigned.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppContributorAssigned))]
    public sealed class AppContributorAssigned : AppEvent
    {
        public string ContributorId { get; set; }

        public AppContributorPermission Permission { get; set; }
    }
}
