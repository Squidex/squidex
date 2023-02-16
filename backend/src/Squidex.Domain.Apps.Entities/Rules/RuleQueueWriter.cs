// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;

namespace Squidex.Domain.Apps.Entities.Rules;

internal sealed class RuleQueueWriter : IAsyncDisposable
{
    private readonly List<RuleEventWrite> writes = new List<RuleEventWrite>();
    private readonly IRuleEventRepository ruleEventRepository;
    private readonly IRuleUsageTracker ruleUsageTracker;
    private readonly ILogger? log;
    private object ruleUsageTracker1;

    public RuleQueueWriter(IRuleEventRepository ruleEventRepository, IRuleUsageTracker ruleUsageTracker, ILogger? log)
    {
        this.ruleEventRepository = ruleEventRepository;
        this.ruleUsageTracker = ruleUsageTracker;
        this.log = log;
    }

    public RuleQueueWriter(IRuleEventRepository ruleEventRepository, object ruleUsageTracker1)
    {
        this.ruleEventRepository = ruleEventRepository;
        this.ruleUsageTracker1 = ruleUsageTracker1;
    }

    public async Task WriteAsync(JobResult result)
    {
        // We do not want to handle disabled rules in the normal flow.
        if (result.Job == null || result.SkipReason is not SkipReason.None and not SkipReason.Failed)
        {
            return;
        }

        if (result.EnrichmentError != null || result.SkipReason is SkipReason.Failed)
        {
            writes.Add(new RuleEventWrite(result.Job, Error: result.EnrichmentError));
        }
        else
        {
            writes.Add(new RuleEventWrite(result.Job, result.Job.Created));
        }

        log?.LogInformation("Adding rule job {jobId} for Rule(action={ruleAction}, trigger={ruleTrigger})",
            result.Job.Id,
            result.Rule.Action.GetType().Name,
            result.Rule.Trigger.GetType().Name);

        var totalFailure = result.SkipReason == SkipReason.Failed ? 1 : 0;
        var totalCreated = 1;

        // Unfortunately we cannot write in batches here, because the result could be from multiple rules.
        await ruleUsageTracker.TrackAsync(result.Job.AppId, result.RuleId, result.Job.Created.ToDateTimeUtc(), totalCreated, 0, totalFailure);

        if (writes.Count >= 100)
        {
            await ruleEventRepository.EnqueueAsync(writes, default);
            writes.Clear();
        }

        return;
    }

    public async ValueTask DisposeAsync()
    {
        if (writes.Count > 0)
        {
            await ruleEventRepository.EnqueueAsync(writes);
            writes.Clear();
        }
    }
}
