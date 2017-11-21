// ==========================================================================
//  RuleDequeuerGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Rules.Orleans.Grains.Implementation
{
    [Reentrant]
    public class RuleDequeuerGrain : Grain, IRuleDequeuerGrain, IRemindable
    {
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly RuleService ruleService;
        private readonly IClock clock;
        private readonly ISemanticLog log;
        private readonly HashSet<Guid> executing = new HashSet<Guid>();
        private TaskFactory scheduler;

        public RuleDequeuerGrain(RuleService ruleService, IRuleEventRepository ruleEventRepository, ISemanticLog log, IClock clock)
            : this(ruleService, ruleEventRepository, log, clock, null, null)
        {
        }

        protected RuleDequeuerGrain(RuleService ruleService, IRuleEventRepository ruleEventRepository, ISemanticLog log, IClock clock,
            IGrainIdentity identity,
            IGrainRuntime runtime)
            : base(identity, runtime)
        {
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(log, nameof(log));

            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;

            this.clock = clock;

            this.log = log;
        }

        public override Task OnActivateAsync()
        {
            scheduler = new TaskFactory(TaskScheduler.Current ?? TaskScheduler.Default);

            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));
            RegisterTimer(x => QueryAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            return base.OnActivateAsync();
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return TaskHelper.Done;
        }

        public Task ActivateAsync()
        {
            return TaskHelper.Done;
        }

        public async Task QueryAsync()
        {
            try
            {
                var self = GetSelf();

                await ruleEventRepository.QueryPendingAsync(clock.GetCurrentInstant(), x =>
                {
                    scheduler.StartNew(() => self.HandleAsync(x.AsImmutable()).Forget()).Forget();

                    return TaskHelper.Done;
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "QueueWebhookEvents")
                    .WriteProperty("status", "Failed"));
            }
        }

        public async Task HandleAsync(Immutable<IRuleEventEntity> @event)
        {
            if (!executing.Add(@event.Value.Id))
            {
                return;
            }

            try
            {
                var job = @event.Value.Job;

                var response = await ruleService.InvokeAsync(job.ActionName, job.ActionData);

                var jobInvoke = ComputeJobInvoke(response.Result, @event, job);
                var jobResult = ComputeJobResult(response.Result, jobInvoke);

                await ruleEventRepository.MarkSentAsync(@event.Value.Id, response.Dump, response.Result, jobResult, response.Elapsed, jobInvoke);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "SendWebhookEvent")
                    .WriteProperty("status", "Failed"));
            }
            finally
            {
                executing.Remove(@event.Value.Id);
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

        private static Instant? ComputeJobInvoke(RuleResult result, Immutable<IRuleEventEntity> @event, RuleJob job)
        {
            if (result != RuleResult.Success)
            {
                switch (@event.Value.NumCalls)
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

        protected virtual IRuleDequeuerGrain GetSelf()
        {
            return this.AsReference<IRuleDequeuerGrain>();
        }
    }
}
