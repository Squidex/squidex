// ==========================================================================
//  RuleDequeuer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Timers;

namespace Squidex.Domain.Apps.Read.Rules
{
    public sealed class RuleDequeuer : DisposableObjectBase, IExternalSystem
    {
        private readonly ActionBlock<IRuleEventEntity> requestBlock;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly RuleService ruleService;
        private readonly CompletionTimer timer;
        private readonly ConcurrentDictionary<Guid, bool> executing = new ConcurrentDictionary<Guid, bool>();
        private readonly IClock clock;
        private readonly ISemanticLog log;

        public RuleDequeuer(RuleService ruleService, IRuleEventRepository ruleEventRepository, ISemanticLog log, IClock clock)
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
                new ActionBlock<IRuleEventEntity>(HandleAsync,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32, BoundedCapacity = 32 });

            timer = new CompletionTimer(5000, QueryAsync);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                timer.StopAsync().Wait();

                requestBlock.Complete();
                requestBlock.Completion.Wait();
            }
        }

        public void Connect()
        {
        }

        public void Next()
        {
            timer.SkipCurrentDelay();
        }

        private async Task QueryAsync(CancellationToken cancellationToken)
        {
            try
            {
                var now = clock.GetCurrentInstant();

                await ruleEventRepository.QueryPendingAsync(now, requestBlock.SendAsync, cancellationToken);
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
    }
}
