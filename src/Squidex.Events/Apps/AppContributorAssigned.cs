// ==========================================================================
//  AppContributorAssigned.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Events.Apps
{
    [TypeName("AppContributorAssignedEvent")]
    public class AppContributorAssigned : AppEvent
    {
        public string ContributorId { get; set; }

        public PermissionLevel Permission { get; set; }
    }
}
