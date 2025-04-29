// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models;

public sealed class DynamicRuleDto : Resource
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
    public DynamicFlowDefinitionDto Flow { get; set; }

    /// <summary>
    /// The action properties.
    /// </summary>
    [Obsolete("Use the new 'Flow' property to define actions. Can be null if the flow cannot be converted.")]
    public Dictionary<string, object> Action { get; set; }

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
}
