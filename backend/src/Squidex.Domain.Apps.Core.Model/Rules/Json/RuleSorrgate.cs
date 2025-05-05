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

        if (ruleDef != null && (!string.Equals(ruleDef.Name, result.Name, StringComparison.Ordinal) || ruleDef.IsEnabled != result.IsEnabled))
        {
            result = result with { Name = result.Name, IsEnabled = ruleDef.IsEnabled };
        }

        var newTrigger = ruleDef?.Trigger ?? Trigger;
        if (newTrigger is IMigrated<RuleTrigger> migratedTrigger)
        {
            newTrigger = migratedTrigger.Migrate();
        }

        if (result.Trigger != newTrigger)
        {
            result = result with { Trigger = newTrigger };
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

            result = result with
            {
                Flow = actualAction.ToFlowDefinition(),
            };
        }

        return result;
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
