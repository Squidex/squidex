// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed class RuleRunnerJob : IJobRunner
{
    public const string TaskName = "run-rule";
    public const string ArgRuleId = "ruleId";
    public const string ArgSnapshot = "snapshots";

    private const int MaxErrors = 10;
    private readonly IAppProvider appProvider;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IRuleEventRepository ruleEventRepository;
    private readonly IRuleService ruleService;
    private readonly IRuleUsageTracker ruleUsageTracker;
    private readonly ILogger<RuleRunnerJob> log;

    public string Name => TaskName;

    public RuleRunnerJob(
        IAppProvider appProvider,
        IEventFormatter eventFormatter,
        IEventStore eventStore,
        IRuleEventRepository ruleEventRepository,
        IRuleService ruleService,
        IRuleUsageTracker ruleUsageTracker,
        ILogger<RuleRunnerJob> log)
    {
        this.appProvider = appProvider;
        this.eventStore = eventStore;
        this.eventFormatter = eventFormatter;
        this.ruleEventRepository = ruleEventRepository;
        this.ruleService = ruleService;
        this.ruleUsageTracker = ruleUsageTracker;
        this.log = log;
    }

    public static DomainId? GetRunningRuleId(Job job)
    {
        if (job.TaskName != TaskName || job.Status != JobStatus.Started)
        {
            return null;
        }

        if (!job.Arguments.TryGetValue(ArgRuleId, out var ruleId))
        {
            return null;
        }

        return DomainId.Create(ruleId);
    }

    public static (string, Dictionary<string, string>) BuildArgs(DomainId ruleId, bool snapshot)
    {
        return (TaskName,
            new Dictionary<string, string>
            {
                [ArgRuleId] = ruleId.ToString(),
                [ArgSnapshot] = snapshot.ToString()
            });
    }

    public async Task RunAsync(JobRun run,
        CancellationToken ct)
    {
        if (!run.Job.Arguments.TryGetValue(ArgRuleId, out var ruleId))
        {
            throw new DomainException("Argument missing.");
        }

        var rule = await appProvider.GetRuleAsync(run.OwnerId, DomainId.Create(ruleId), ct)
            ?? throw new DomainObjectNotFoundException(ruleId);

        var fromSnapshot = string.Equals(run.Job.Arguments.GetOrAddDefault(ArgSnapshot), "true", StringComparison.OrdinalIgnoreCase);

        // Also run disabled rules, because we want to enable rules to be only used with manual trigger.
        var context = new RuleContext
        {
            AppId = rule.AppId,
            IncludeStale = true,
            IncludeSkipped = true,
            Rule = rule,
        };

        if (fromSnapshot && ruleService.CanCreateSnapshotEvents(rule))
        {
            await EnqueueFromSnapshotsAsync(context, ct);
        }
        else
        {
            await EnqueueFromEventsAsync(run, context, ct);
        }
    }

    private async Task EnqueueFromSnapshotsAsync(RuleContext context,
        CancellationToken ct)
    {
        // We collect errors and allow a few erors before we throw an exception.
        var errors = 0;

        // Write in batches of 100 items for better performance. Using completes the last write.
        await using var batch = new RuleQueueWriter(ruleEventRepository, ruleUsageTracker, null);

        await foreach (var result in ruleService.CreateSnapshotJobsAsync(context, ct))
        {
            await batch.WriteAsync(result);

            if (result.EnrichmentError != null)
            {
                errors++;

                // We accept a few errors and stop the process if there are too many errors.
                if (errors >= MaxErrors)
                {
                    throw result.EnrichmentError;
                }

                log.LogWarning(result.EnrichmentError, "Failed to run rule with ID {ruleId}, continue with next job.", result.Rule?.Id);
            }
        }
    }

    private async Task EnqueueFromEventsAsync(JobRun run, RuleContext context,
        CancellationToken ct)
    {
        // We collect errors and allow a few erors before we throw an exception.
        var errors = 0;

        // Write in batches of 100 items for better performance. Using completes the last write.
        await using var batch = new RuleQueueWriter(ruleEventRepository, ruleUsageTracker, null);

        // Use a prefix query so that the storage can use an index for the query.
        var streamFilter = StreamFilter.Prefix($"([a-zA-Z0-9]+)\\-{run.OwnerId}");

        await foreach (var storedEvent in eventStore.QueryAllAsync(streamFilter, ct: ct))
        {
            var @event = eventFormatter.ParseIfKnown(storedEvent);

            if (@event == null)
            {
                continue;
            }

            await foreach (var result in ruleService.CreateJobsAsync(@event, context.ToRulesContext(), ct))
            {
                await batch.WriteAsync(result);

                if (result.EnrichmentError != null)
                {
                    errors++;

                    // We accept a few errors and stop the process if there are too many errors.
                    if (errors >= MaxErrors)
                    {
                        throw result.EnrichmentError;
                    }

                    log.LogWarning(result.EnrichmentError, "Failed to run rule with ID {ruleId}, continue with next job.", result.Rule?.Id);
                }
            }
        }

        await batch.FlushAsync();
    }
}
