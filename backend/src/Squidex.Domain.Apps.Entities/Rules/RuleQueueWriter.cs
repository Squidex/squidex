// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleQueueWriter(IFlowManager<FlowEventContext> flowManager, IRuleUsageTracker ruleUsageTracker, ILogger? log)
    : IAsyncDisposable
{
    private readonly List<CreateFlowInstanceRequest<FlowEventContext>> writes = [];

    public async Task<bool> WriteAsync(DomainId appId, JobResult result)
    {
        Guard.NotNull(result);

        // We do not want to handle events without a job in the normal flow.
        if (result.Job == null || result.Rule == null)
        {
            return false;
        }

        var job = result.Job.Value;

        writes.Add(job);

        if (result.Rule != null)
        {
            log?.LogInformation("Adding rule job for Rule(trigger={ruleTrigger})",
                result.Rule.Trigger.GetType().Name);
        }

        var totalFailure = result.SkipReason == SkipReason.Failed ? 1 : 0;
        var totalCreated = 1;
        var totalSucceeded = 0;

        // Unfortunately we cannot write in batches here, because the result could be from multiple rules.
        await ruleUsageTracker.TrackAsync(
            appId,
            result.Rule?.Id ?? default,
            DateTime.UtcNow.ToDateOnly(),
            totalCreated,
            totalSucceeded,
            totalFailure);

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
        await flowManager.EnqueueAsync(writes.ToArray(), default);
        writes.Clear();
    }
}
