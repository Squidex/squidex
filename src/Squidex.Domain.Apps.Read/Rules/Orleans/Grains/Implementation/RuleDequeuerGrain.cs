// ==========================================================================
//  RuleDequeuerGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NodaTime;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Rules.Orleans.Grains.Implementation
{
    public class RuleDequeuerGrain : Grain, IRuleDequeuerGrain, IRemindable
    {
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly RuleService ruleService;
        private readonly IClock clock;
        private readonly ISemanticLog log;
        private ActionBlock<IRuleEventEntity> requestBlock;
        private TransformBlock<IRuleEventEntity, IRuleEventEntity> blockBlock;

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
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));
            RegisterTimer(x => QueryAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            requestBlock =
                new ActionBlock<IRuleEventEntity>(MakeRequestAsync,
                    new ExecutionDataflowBlockOptions
                    {
                        TaskScheduler = TaskScheduler.Current,
                        MaxMessagesPerTask = 1,
                        MaxDegreeOfParallelism = 32,
                        BoundedCapacity = 32
                    });

            blockBlock =
                new TransformBlock<IRuleEventEntity, IRuleEventEntity>(x => BlockAsync(x),
                    new ExecutionDataflowBlockOptions
                    {
                        TaskScheduler = TaskScheduler.Current,
                        MaxMessagesPerTask = 1,
                        MaxDegreeOfParallelism = 1,
                        BoundedCapacity = 1
                    });

            blockBlock.LinkTo(requestBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return base.OnActivateAsync();
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return QueryAsync();
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

                await ruleEventRepository.QueryPendingAsync(now, blockBlock.SendAsync, CancellationToken.None);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "QueueWebhookEvents")
                    .WriteProperty("status", "Failed"));
            }
        }

        private async Task<IRuleEventEntity> BlockAsync(IRuleEventEntity @event)
        {
            try
            {
                await ruleEventRepository.MarkSendingAsync(@event.Id);

                return @event;
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "BlockWebhookEvent")
                    .WriteProperty("status", "Failed"));

                throw;
            }
        }

        private async Task MakeRequestAsync(IRuleEventEntity @event)
        {
            try
            {
                var job = @event.Job;

                var response = await ruleService.InvokeAsync(job.ActionName, job.ActionData);

                Instant? nextCall = null;

                if (response.Result != RuleResult.Success)
                {
                    switch (@event.NumCalls)
                    {
                        case 0:
                            nextCall = job.Created.Plus(Duration.FromMinutes(5));
                            break;
                        case 1:
                            nextCall = job.Created.Plus(Duration.FromHours(1));
                            break;
                        case 2:
                            nextCall = job.Created.Plus(Duration.FromHours(6));
                            break;
                        case 3:
                            nextCall = job.Created.Plus(Duration.FromHours(12));
                            break;
                    }
                }

                RuleJobResult jobResult;

                if (response.Result != RuleResult.Success && !nextCall.HasValue)
                {
                    jobResult = RuleJobResult.Failed;
                }
                else if (response.Result != RuleResult.Success && nextCall.HasValue)
                {
                    jobResult = RuleJobResult.Retry;
                }
                else
                {
                    jobResult = RuleJobResult.Success;
                }

                await ruleEventRepository.MarkSentAsync(@event.Id, response.Dump, response.Result, jobResult, response.Elapsed, nextCall);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "SendWebhookEvent")
                    .WriteProperty("status", "Failed"));

                throw;
            }
        }
    }
}
