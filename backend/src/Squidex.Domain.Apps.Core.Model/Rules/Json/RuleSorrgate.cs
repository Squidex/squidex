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

public sealed record RuleSorrgate : Rule, ISurrogate<Rule>
{
    public void FromSource(Rule source)
    {
        SimpleMapper.Map(source, this);
    }

    public Rule ToSource()
    {
        if (Trigger is IMigrated<RuleTrigger> migrated)
        {
            return this with { Trigger = migrated.Migrate() };
        }

        return this;
    }
}
