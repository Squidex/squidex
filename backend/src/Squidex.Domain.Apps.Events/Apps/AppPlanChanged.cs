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
    [EventType(nameof(AppPlanChanged))]
    public sealed class AppPlanChanged : AppEvent
    {
        public string PlanId { get; set; }

        public AppPlan ToAppPlan()
        {
            return new AppPlan(Actor, PlanId);
        }
    }
}
