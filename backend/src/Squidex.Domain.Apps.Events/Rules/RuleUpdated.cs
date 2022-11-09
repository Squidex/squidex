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

[EventType(nameof(RuleUpdated))]
public sealed class RuleUpdated : RuleEvent, IMigrated<IEvent>
{
    public string? Name { get; set; }

    public RuleTrigger? Trigger { get; set; }

    public RuleAction? Action { get; set; }

    public bool? IsEnabled { get; set; }

    public IEvent Migrate()
    {
        if (Trigger is IMigrated<RuleTrigger> migrated)
        {
            var clone = (RuleUpdated)MemberwiseClone();

            clone.Trigger = migrated.Migrate();

            return clone;
        }

        return this;
    }
}
