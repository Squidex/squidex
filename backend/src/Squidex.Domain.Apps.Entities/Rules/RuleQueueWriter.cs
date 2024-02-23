// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleQueueWriter : IAsyncDisposable
{
    private readonly List<RuleEventWrite> writes = [];
    private readonly IRuleEventRepository ruleEventRepository;
    private readonly IRuleUsageTracker ruleUsageTracker;
    private readonly ILogger? log;

    public RuleQueueWriter(IRuleEventRepository ruleEventRepository, IRuleUsageTracker ruleUsageTracker, ILogger? log)
    {
        this.ruleEventRepository = ruleEventRepository;
        this.ruleUsageTracker = ruleUsageTracker;
        this.log = log;
    }

    public async Task<bool> WriteAsync(JobResult result)
    {
        // We do not want to handle events without a job in the normal flow.
        if (result.Job == null)
        {
            return false;
        }

        if (result.EnrichmentError != null || result.SkipReason is SkipReason.Failed)
        {
            writes.Add(new RuleEventWrite(result.Job, Error: result.EnrichmentError));
        }
        else if (result.SkipReason is SkipReason.None or SkipReason.Disabled)
        {
            writes.Add(new RuleEventWrite(result.Job, result.Job.Created));
        }
        else
        {
            return false;
        }

        if (result.Rule != null)
        {
            log?.LogInformation("Adding rule job {jobId} for Rule(action={ruleAction}, trigger={ruleTrigger})",
                result.Job.Id,
                result.Rule.Action.GetType().Name,
                result.Rule.Trigger.GetType().Name);
        }

        var totalFailure = result.SkipReason == SkipReason.Failed ? 1 : 0;
        var totalCreated = 1;

        // Unfortunately we cannot write in batches here, because the result could be from multiple rules.
        await ruleUsageTracker.TrackAsync(result.Job.AppId, result.Rule?.Id ?? default, result.Job.Created.ToDateOnly(), totalCreated, 0, totalFailure);

        if (writes.Count >= 100)
        {
            await FlushCoreAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> FlushAsync()
    {
        if (writes.Count > 0)
        {
            await FlushCoreAsync();
            return true;
        }

        return false;
    }

    public async ValueTask DisposeAsync()
    {
        if (writes.Count > 0)
        {
            await FlushCoreAsync();
        }
    }

    private async Task FlushCoreAsync()
    {
        await ruleEventRepository.EnqueueAsync(writes, default);
        writes.Clear();
    }
}
