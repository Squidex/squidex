// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Domain.Apps.Events.Rules;

[EventType(nameof(RuleCreated))]
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

        return this;
    }
}