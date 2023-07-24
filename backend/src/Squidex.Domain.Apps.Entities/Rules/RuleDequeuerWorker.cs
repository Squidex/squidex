// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleDequeuerWorker : IBackgroundProcess
{
    private readonly ConcurrentDictionary<DomainId, bool> executing = new ConcurrentDictionary<DomainId, bool>();
    private readonly PartitionedScheduler<IRuleEventEntity> requestScheduler;
    private readonly IRuleEventRepository ruleEventRepository;
    private readonly IRuleService ruleService;
    private readonly IRuleUsageTracker ruleUsageTracker;
    private readonly ILogger<RuleDequeuerWorker> log;
    private CompletionTimer timer;

    public IClock Clock { get; set; } = SystemClock.Instance;

    public RuleDequeuerWorker(
        IRuleService ruleService,
        IRuleUsageTracker ruleUsageTracker,
        IRuleEventRepository ruleEventRepository,
        ILogger<RuleDequeuerWorker> log)
    {
        this.ruleEventRepository = ruleEventRepository;
        this.ruleService = ruleService;
        this.ruleUsageTracker = ruleUsageTracker;
        this.log = log;

        requestScheduler = new PartitionedScheduler<IRuleEventEntity>(HandleAsync, 32, 2);
    }

    public Task StartAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer((int)TimeSpan.FromSeconds(10).TotalMilliseconds, QueryAsync);

        return Task.CompletedTask;
    }

    public async Task StopAsync(
        CancellationToken ct)
    {
        await (timer?.StopAsync() ?? Task.CompletedTask);

        await requestScheduler.CompleteAsync();
    }

    public async Task QueryAsync(
        CancellationToken ct = default)
    {
        try
        {
            var now = Clock.GetCurrentInstant();

            await ruleEventRepository.QueryPendingAsync(now, async @event =>
            {
                await requestScheduler.ScheduleAsync(@event.Job.ExecutionPartition, @event, ct);
            }, ct);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to query rule events.");
        }
    }

    public async Task HandleAsync(IRuleEventEntity @event,
        CancellationToken ct)
    {
        if (!executing.TryAdd(@event.Id, false))
        {
            return;
        }

        try
        {
            var job = @event.Job;

            var (response, elapsed) = await ruleService.InvokeAsync(job.ActionName, job.ActionData, ct);

            var jobDelay = ComputeJobDelay(response.Status, @event, job);
            var jobResult = ComputeJobResult(response.Status, jobDelay);

            var now = Clock.GetCurrentInstant();

            var update = new RuleJobUpdate
            {
                Elapsed = elapsed,
                ExecutionDump = response.Dump,
                ExecutionResult = response.Status,
                Finished = now,
                JobNext = jobDelay,
                JobResult = jobResult
            };

            await ruleEventRepository.UpdateAsync(@event.Job, update, default);

            if (response.Status == RuleResult.Failed)
            {
                await ruleUsageTracker.TrackAsync(job.AppId, job.RuleId, now.ToDateOnly(), 0, 0, 1, default);

                log.LogWarning(response.Exception, "Failed to execute rule event with rule id {ruleId}/{description}.",
                    @event.Job.RuleId,
                    @event.Job.Description);
            }
            else
            {
                await ruleUsageTracker.TrackAsync(job.AppId, job.RuleId, now.ToDateOnly(), 0, 1, 0, default);
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to execute rule event with internal error.");
        }
        finally
        {
            executing.TryRemove(@event.Id, out _);
        }
    }

    private static RuleJobResult ComputeJobResult(RuleResult result, Instant? nextCall)
    {
        if (result != RuleResult.Success && nextCall == null)
        {
            return RuleJobResult.Failed;
        }
        else if (result != RuleResult.Success && nextCall != null)
        {
            return RuleJobResult.Retry;
        }
        else
        {
            return RuleJobResult.Success;
        }
    }

    private static Instant? ComputeJobDelay(RuleResult result, IRuleEventEntity @event, RuleJob job)
    {
        if (result != RuleResult.Success)
        {
            switch (@event.NumCalls)
            {
                case 0:
                    return job.Created.Plus(Duration.FromMinutes(5));
                case 1:
                    return job.Created.Plus(Duration.FromHours(1));
                case 2:
                    return job.Created.Plus(Duration.FromHours(6));
                case 3:
                    return job.Created.Plus(Duration.FromHours(12));
            }
        }

        return null;
    }
}
