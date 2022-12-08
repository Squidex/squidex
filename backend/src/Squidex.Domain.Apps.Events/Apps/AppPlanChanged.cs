// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps;

[EventType(nameof(AppPlanChanged))]
public sealed class AppPlanChanged : AppEvent
{
    public string PlanId { get; set; }

    public AssignedPlan ToPlan()
    {
        return new AssignedPlan(Actor, PlanId);
    }
}
