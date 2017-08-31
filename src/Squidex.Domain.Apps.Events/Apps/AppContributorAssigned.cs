// ==========================================================================
//  AppContributorAssigned.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppContributorAssignedEvent")]
    public sealed class AppContributorAssigned : AppEvent
    {
        public string ContributorId { get; set; }

        public PermissionLevel Permission { get; set; }
    }
}
