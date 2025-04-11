// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Flows.Internal;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using RuleCreatedV2 = Squidex.Domain.Apps.Events.Rules.RuleCreated;

namespace Migrations.OldEvents;

[EventType(nameof(RuleCreated))]
[Obsolete("New Event introduced")]
public sealed class RuleCreated : RuleEvent, IMigrated<IEvent>
{
    public RuleTrigger Trigger { get; set; }

    public RuleAction Action { get; set; }

    public string Name { get; set; }

    public IEvent Migrate()
    {
        if (Trigger is IMigrated<RuleTrigger> migrated)
        {
            Trigger = migrated.Migrate();
        }

        return SimpleMapper.Map(this, new RuleCreatedV2
        {
            Flow = new FlowDefinition
            {
                Steps = new Dictionary<Guid, FlowStepDefinition>
                {
                    [Guid.Empty] = new FlowStepDefinition
                    {
                        Step = Action.ToFlowStep(),
                    },
                },
                InitialStep = Guid.Empty,
            },
        });
    }
}
