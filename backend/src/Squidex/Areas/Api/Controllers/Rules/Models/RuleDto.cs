﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Areas.Api.Controllers.Rules.Models.Converters;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class RuleDto : Resource
{
    /// <summary>
    /// The ID of the rule.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The user that has created the rule.
    /// </summary>
    public RefToken CreatedBy { get; set; }

    /// <summary>
    /// The user that has updated the rule.
    /// </summary>
    public RefToken LastModifiedBy { get; set; }

    /// <summary>
    /// The date and time when the rule has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The date and time when the rule has been modified last.
    /// </summary>
    public Instant LastModified { get; set; }

    /// <summary>
    /// The version of the rule.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Determines if the rule is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Optional rule name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The trigger properties.
    /// </summary>
    public RuleTriggerDto Trigger { get; set; }

    /// <summary>
    /// The flow to describe the sequence of actions to perform.
    /// </summary>
    public FlowDefinitionDto Flow { get; set; }

    /// <summary>
    /// The action properties.
    /// </summary>
    [Obsolete("Use the new 'Flow' property to define actions. Can be null if the flow cannot be converted.")]
    public RuleAction Action { get; set; }

    /// <summary>
    /// The number of completed executions.
    /// </summary>
    public long NumSucceeded { get; set; }

    /// <summary>
    /// The number of failed executions.
    /// </summary>
    public long NumFailed { get; set; }

    /// <summary>
    /// The date and time when the rule was executed the last time.
    /// </summary>
    [Obsolete("Removed when migrated to new rule statistics.")]
    public Instant? LastExecuted { get; set; }

    public static RuleDto FromDomain(EnrichedRule rule, bool canRun, IRuleRunnerService ruleRunnerService, Resources resources)
    {
        var result = SimpleMapper.Map(rule, new RuleDto());

        if (rule.Trigger != null)
        {
            result.Trigger = RuleTriggerDtoFactory.Create(rule.Trigger);
        }

        if (rule.Flow != null)
        {
            result.Flow = FlowDefinitionDto.FromDomain(rule.Flow);
        }

        if (rule.Flow != null && rule.Flow.Steps.Count == 1)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (rule.Flow.Steps.First().Value.Step is IConvertibleToAction convertible)
            {
                result.Action = convertible.ToAction();
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        return result.CreateLinks(resources, rule, canRun, ruleRunnerService);
    }

    private RuleDto CreateLinks(Resources resources, EnrichedRule rule, bool canRun, IRuleRunnerService ruleRunnerService)
    {
        var values = new { app = resources.App, id = Id };

        if (resources.CanDisableRule)
        {
            if (IsEnabled)
            {
                AddPutLink("disable",
                    resources.Url<RulesController>(x => nameof(x.DisableRule), values));
            }
            else
            {
                AddPutLink("enable",
                    resources.Url<RulesController>(x => nameof(x.EnableRule), values));
            }
        }

        if (resources.CanUpdateRule)
        {
            AddPutLink("update",
                resources.Url<RulesController>(x => nameof(x.PutRule), values));
        }

        if (resources.CanRunRuleEvents)
        {
            if (rule.Trigger is ManualTrigger)
            {
                AddPutLink("trigger",
                    resources.Url<RulesController>(x => nameof(x.TriggerRule), values));
            }

            if (canRun && ruleRunnerService.CanRunRule(rule))
            {
                AddPutLink("run",
                    resources.Url<RulesController>(x => nameof(x.PutRuleRun), values));
            }

            if (canRun && ruleRunnerService.CanRunFromSnapshots(rule))
            {
                var snaphshotValues = new { values.app, values.id, fromSnapshots = true };

                AddPutLink("run/snapshots",
                    resources.Url<RulesController>(x => nameof(x.PutRuleRun), snaphshotValues));
            }
        }

        if (resources.CanReadRuleEvents)
        {
            AddGetLink("logs",
                resources.Url<RulesController>(x => nameof(x.GetEvents), values));
        }

        if (resources.CanDeleteRule)
        {
            AddDeleteLink("delete",
                resources.Url<RulesController>(x => nameof(x.DeleteRule), values));
        }

        return this;
    }
}
