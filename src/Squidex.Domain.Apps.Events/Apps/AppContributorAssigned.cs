// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppContributorAssigned))]
    public sealed class AppContributorAssigned : AppEvent
    {
        public string ContributorId { get; set; }

        public AppContributorPermission Permission { get; set; }
    }
}
