// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using NodaTime;
using Orleans;
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDequeuerGrain : Grain, IRuleDequeuerGrain, IRemindable
    {
        private readonly ITargetBlock<IRuleEventEntity> requestBlock;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IRuleService ruleService;
        private readonly ConcurrentDictionary<DomainId, bool> executing = new ConcurrentDictionary<DomainId, bool>();
        private readonly IClock clock;
        private readonly ILogger<RuleDequeuerGrain> log;

        public RuleDequeuerGrain(
            IRuleService ruleService,
            IRuleEventRepository ruleEventRepository,
            ILogger<RuleDequeuerGrain> log,
            IClock clock)
        {
            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;

            this.clock = clock;

            this.log = log;

            requestBlock =
                new PartitionedActionBlock<IRuleEventEntity>(HandleAsync, x => x.Job.ExecutionPartition,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32, BoundedCapacity = 32 });
        }

        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));
            RegisterTimer(x => QueryAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            requestBlock.Complete();

            return requestBlock.Completion;
        }

        public Task ActivateAsync()
        {
            return Task.CompletedTask;
        }

        public async Task QueryAsync()
        {
            try
            {
                var now = clock.GetCurrentInstant();

                await ruleEventRepository.QueryPendingAsync(now, requestBlock.SendAsync);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to query rule events.");
            }
        }

        public async Task HandleAsync(IRuleEventEntity @event)
        {
            if (!executing.TryAdd(@event.Id, false))
            {
                return;
            }

            try
            {
                var job = @event.Job;

                var (response, elapsed) = await ruleService.InvokeAsync(job.ActionName, job.ActionData);

                var jobDelay = ComputeJobDelay(response.Status, @event, job);
                var jobResult = ComputeJobResult(response.Status, jobDelay);

                var now = clock.GetCurrentInstant();

                var update = new RuleJobUpdate
                {
                    Elapsed = elapsed,
                    ExecutionDump = response.Dump,
                    ExecutionResult = response.Status,
                    Finished = now,
                    JobNext = jobDelay,
                    JobResult = jobResult
                };

                await ruleEventRepository.UpdateAsync(@event.Job, update);

                if (response.Status == RuleResult.Failed)
                {
                    log.LogWarning(response.Exception, "Failed to execute rule event with rule id {ruleId}/{description}.",
                        @event.Job.RuleId,
                        @event.Job.Description);
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

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return Task.CompletedTask;
        }
    }
}
