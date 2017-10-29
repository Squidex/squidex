// ==========================================================================
//  RuleDequeuer.cs
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
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Timers;

namespace Squidex.Domain.Apps.Read.Rules
{
    public sealed class RuleDequeuer : DisposableObjectBase, IExternalSystem
    {
        private readonly ActionBlock<IRuleEventEntity> requestBlock;
        private readonly TransformBlock<IRuleEventEntity, IRuleEventEntity> blockBlock;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly RuleService ruleService;
        private readonly CompletionTimer timer;
        private readonly ISemanticLog log;

        public RuleDequeuer(RuleService ruleService, IRuleEventRepository ruleEventRepository, ISemanticLog log)
        {
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));
            Guard.NotNull(log, nameof(log));

            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;

            this.log = log;

            requestBlock =
                new ActionBlock<IRuleEventEntity>(MakeRequestAsync,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32, BoundedCapacity = 32 });

            blockBlock =
                new TransformBlock<IRuleEventEntity, IRuleEventEntity>(x => BlockAsync(x),
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 1 });

            blockBlock.LinkTo(requestBlock, new DataflowLinkOptions { PropagateCompletion = true });

            timer = new CompletionTimer(5000, QueryAsync);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                timer.StopAsync().Wait();

                blockBlock.Complete();
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
                await ruleEventRepository.QueryPendingAsync(blockBlock.SendAsync, cancellationToken);
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

                await ruleEventRepository.MarkSentAsync(@event.Id, response.Dump, response.Result, response.Elapsed, nextCall);
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
