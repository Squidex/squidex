// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Teams;

[EventType(nameof(TeamPlanChanged))]
public sealed class TeamPlanChanged : TeamEvent
{
    public string PlanId { get; set; }
}
