// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NodaTime;
using Orleans;
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDequeuerGrain : Grain, IRuleDequeuerGrain, IRemindable
    {
        private readonly ITargetBlock<IRuleEventEntity> requestBlock;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly RuleService ruleService;
        private readonly ConcurrentDictionary<Guid, bool> executing = new ConcurrentDictionary<Guid, bool>();
        private readonly IClock clock;
        private readonly ISemanticLog log;

        public RuleDequeuerGrain(RuleService ruleService, IRuleEventRepository ruleEventRepository, ISemanticLog log, IClock clock)
        {
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(log, nameof(log));

            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;

            this.clock = clock;

            this.log = log;

            requestBlock =
                new PartitionedActionBlock<IRuleEventEntity>(HandleAsync, x => x.Job.AggregateId.GetHashCode(),
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32, BoundedCapacity = 32 });
        }

        public override Task OnActivateAsync()
        {
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));
            RegisterTimer(x => QueryAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            return Task.FromResult(true);
        }

        public override Task OnDeactivateAsync()
        {
            requestBlock.Complete();

            return requestBlock.Completion;
        }

        public Task ActivateAsync()
        {
            return TaskHelper.Done;
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
                log.LogError(ex, w => w
                    .WriteProperty("action", "QueueWebhookEvents")
                    .WriteProperty("status", "Failed"));
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

                var response = await ruleService.InvokeAsync(job.ActionName, job.ActionData);

                var jobInvoke = ComputeJobInvoke(response.Result, @event, job);
                var jobResult = ComputeJobResult(response.Result, jobInvoke);

                await ruleEventRepository.MarkSentAsync(@event.Id, response.Dump, response.Result, jobResult, response.Elapsed, jobInvoke);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "SendWebhookEvent")
                    .WriteProperty("status", "Failed"));
            }
            finally
            {
                executing.TryRemove(@event.Id, out var value);
            }
        }

        private static RuleJobResult ComputeJobResult(RuleResult result, Instant? nextCall)
        {
            if (result != RuleResult.Success && !nextCall.HasValue)
            {
                return RuleJobResult.Failed;
            }
            else if (result != RuleResult.Success && nextCall.HasValue)
            {
                return RuleJobResult.Retry;
            }
            else
            {
                return RuleJobResult.Success;
            }
        }

        private static Instant? ComputeJobInvoke(RuleResult result, IRuleEventEntity @event, RuleJob job)
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
            return TaskHelper.Done;
        }
    }
}
