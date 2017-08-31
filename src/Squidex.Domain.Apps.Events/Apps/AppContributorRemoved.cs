// ==========================================================================
//  AppContributorRemoved.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppContributorRemovedEvent")]
    public sealed class AppContributorRemoved : AppEvent
    {
        public string ContributorId { get; set; }
    }
}
