// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Json;

public sealed class RuleSorrgate : ISurrogate<Rule>
{
    public RuleTrigger Trigger { get; set; }

    public RuleAction Action { get; set; }

    public bool IsEnabled { get; set; }

    public string Name { get; set; }

    public void FromSource(Rule source)
    {
        SimpleMapper.Map(source, this);
    }

    public Rule ToSource()
    {
        var trigger = Trigger;

        if (trigger is IMigrated<RuleTrigger> migrated)
        {
            trigger = migrated.Migrate();
        }

        var rule = new Rule(trigger, Action);

        if (!IsEnabled)
        {
            rule = rule.Disable();
        }

        if (Name != null)
        {
            rule = rule.Rename(Name);
        }

        return rule;
    }
}
