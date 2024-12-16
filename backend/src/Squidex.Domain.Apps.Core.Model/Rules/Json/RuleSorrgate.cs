// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core.Rules.Old;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Json;

public sealed record RuleSorrgate : Rule, ISurrogate<Rule>
{
    [Obsolete("Old serialization format.")]
    private Rule? ruleDef;

    [Obsolete("Old rule action system.")]
    public RuleAction? Action { get; set; }

    [JsonPropertyName("ruleDef")]
    [Obsolete("Old serialization format.")]
    public Rule? RuleDef
    {
        // Because this property is old we old want to read it and never to write it.
        set => ruleDef = value;
    }

    public void FromSource(Rule source)
    {
        SimpleMapper.Map(source, this);
    }

    public Rule ToSource()
    {
        Rule result = this;

#pragma warning disable CS0618 // Type or member is obsolete
        if (ruleDef != null)
        {
            // In previous versions, the actual rule was stored in a nested object.
            result = ruleDef with
            {
                Id = Id,
                AppId = AppId,
                Created = Created,
                CreatedBy = CreatedBy,
                IsDeleted = IsDeleted,
                LastModified = LastModified,
                LastModifiedBy = LastModifiedBy,
                Version = Version,
            };
        }

        if (Action != null)
        {
            var action = Action;
            if (action is IMigrated<RuleAction> migratedAction)
            {
                action = migratedAction.Migrate();
            }

            result = result with
            {
                Flow = action.ToFlow()
            };
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (result.Trigger is IMigrated<RuleTrigger> migratedTrigger)
        {
            result = result with { Trigger = migratedTrigger.Migrate() };
        }

        return result;
    }
}
