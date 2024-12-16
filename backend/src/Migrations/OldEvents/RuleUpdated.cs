// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Old;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using RuleUpdatedV2 = Squidex.Domain.Apps.Events.Rules.RuleUpdated;

namespace Migrations.OldEvents;

[EventType(nameof(RuleUpdated))]
public sealed class RuleUpdated : RuleEvent, IMigrated<IEvent>
{
    public string? Name { get; set; }

    public RuleTrigger? Trigger { get; set; }

    public RuleAction? Action { get; set; }

    public bool? IsEnabled { get; set; }

    public IEvent Migrate()
    {
        var action = Action;
        if (action is IMigrated<RuleAction> migratedAction)
        {
            action = migratedAction.Migrate();
        }

        var migrated = SimpleMapper.Map(this, new RuleUpdatedV2
        {
            Flow = action?.ToFlow()
        });

        return migrated.Migrate();
    }
}
