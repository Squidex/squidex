// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows.Execution;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleQueueWriter(IFlowStateStore<RuleFlowContext> flowStore, IRuleUsageTracker ruleUsageTracker, ILogger? log)
    : IAsyncDisposable
{
    private readonly List<FlowExecutionState<RuleFlowContext>> writes = [];

    public IClock Clock { get; set; } = SystemClock.Instance;

    public async Task<bool> WriteAsync(JobResult result)
    {
        // We do not want to handle events without a job in the normal flow.
        if (result.State == null)
        {
            return false;
        }

        if (result.State != null)
        {
            writes.Add(result.State);
        }
        else
        {
            return false;
        }

        log?.LogInformation("Adding rule job {jobId} for Rule(trigger={ruleTrigger})",
            result.State.InstanceId,
            result.Rule.Trigger.GetType().Name);

        var totalFailure = result.SkipReason == SkipReason.Failed ? 1 : 0;
        var totalCreated = 1;

        // Unfortunately we cannot write in batches here, because the result could be from multiple rules.
        await ruleUsageTracker.TrackAsync(
            result.Rule.AppId.Id,
            result.Rule.Id,
            Clock.GetCurrentInstant().ToDateOnly(),
            totalCreated,
            0,
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
        await flowStore.StoreAsync(writes, default);
        writes.Clear();
    }
}
