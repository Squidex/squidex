// ==========================================================================
//  AppContributorAssigned.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppContributorAssigned")]
    public class AppContributorAssigned : IEvent
    {
        public string SubjectId { get; set; }

        public PermissionLevel Permission { get; set; }
    }
}
