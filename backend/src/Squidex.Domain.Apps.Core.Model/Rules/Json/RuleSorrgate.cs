// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Json;

public sealed record RuleSorrgate : Rule, ISurrogate<Rule>
{
    [Obsolete("Old serialization format.")]
    private DeprecatedRule? ruleDef;

    [Obsolete("Old rule system.")]
    private RuleAction? oldAction;

    [JsonPropertyName("action")]
    [Obsolete("Old rule system.")]
    public RuleAction Action
    {
        // Because this property is old we old want to read it and never to write it.
        set => oldAction = value;
    }

    [JsonPropertyName("ruleDef")]
    [Obsolete("Old serialization format.")]
    public DeprecatedRule? RuleDef
    {
        // Because this property is old we old want to read it and never to write it.
        set => ruleDef = value;
    }

    public void FromSource(Rule source)
    {
        SimpleMapper.Map(source, this);
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public Rule ToSource()
    {
        var result = this;

        if (ruleDef != null)
        {
            result = this with { Name = result.Name, IsEnabled = ruleDef.IsEnabled, Trigger = ruleDef.Trigger };
        }

        if (result.Flow == null)
        {
            var actualAction = ruleDef != null ? ruleDef.Action : oldAction;

            if (actualAction == null || result.Trigger == null)
            {
                throw new InvalidOperationException("Neither a flow, nor trigger and action is defined.");
            }

            if (actualAction is IMigrated<RuleAction> migratedAction)
            {
                actualAction = migratedAction.Migrate();
            }

            result = this with
            {
                Flow = actualAction.ToFlowDefinition(),
            };
        }

        if (result.Trigger is IMigrated<RuleTrigger> migratedTrigger)
        {
            return this with { Trigger = migratedTrigger.Migrate() };
        }

        return result;
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
