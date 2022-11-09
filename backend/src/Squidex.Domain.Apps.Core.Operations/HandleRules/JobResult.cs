// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed record JobResult
{
    public static readonly JobResult ConditionDoesNotMatch = new JobResult
    {
        SkipReason = SkipReason.ConditionDoesNotMatch
    };

    public static readonly JobResult ConditionPrecheckDoesNotMatch = new JobResult
    {
        SkipReason = SkipReason.ConditionPrecheckDoesNotMatch
    };

    public static readonly JobResult Disabled = new JobResult
    {
        SkipReason = SkipReason.Disabled
    };

    public static readonly JobResult WrongEvent = new JobResult
    {
        SkipReason = SkipReason.WrongEvent
    };

    public static readonly JobResult FromRule = new JobResult
    {
        SkipReason = SkipReason.FromRule
    };

    public static readonly JobResult NoAction = new JobResult
    {
        SkipReason = SkipReason.NoAction
    };

    public static readonly JobResult NoTrigger = new JobResult
    {
        SkipReason = SkipReason.NoTrigger
    };

    public static readonly JobResult TooOld = new JobResult
    {
        SkipReason = SkipReason.TooOld
    };

    public static readonly JobResult WrongEventForTrigger = new JobResult
    {
        SkipReason = SkipReason.WrongEventForTrigger
    };

    public RuleJob? Job { get; init; }

    public EnrichedEvent? EnrichedEvent { get; init; }

    public Exception? EnrichmentError { get; init; }

    public SkipReason SkipReason { get; init; }

    public static JobResult Failed(Exception exception, EnrichedEvent? enrichedEvent = null, RuleJob? job = null)
    {
        return new JobResult
        {
            Job = job,
            EnrichedEvent = enrichedEvent,
            EnrichmentError = exception,
            SkipReason = SkipReason.Failed
        };
    }
}
