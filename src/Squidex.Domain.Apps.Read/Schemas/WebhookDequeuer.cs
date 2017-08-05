// ==========================================================================
//  WebhookDequeuer.cs
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
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Timers;

// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable MethodSupportsCancellation
// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Read.Schemas
{
    public sealed class WebhookDequeuer : DisposableObjectBase, IExternalSystem
    {
        private readonly ActionBlock<IWebhookEventEntity> requestBlock;
        private readonly TransformBlock<IWebhookEventEntity, IWebhookEventEntity> blockBlock;
        private readonly IWebhookEventRepository webhookEventRepository;
        private readonly ISchemaWebhookRepository webhookRepository;
        private readonly WebhookSender webhookSender;
        private readonly CompletionTimer timer;
        private readonly ISemanticLog log;
        private readonly IClock clock;

        public WebhookDequeuer(WebhookSender webhookSender,
            IWebhookEventRepository webhookEventRepository,
            ISchemaWebhookRepository webhookRepository,
            IClock clock,
            ISemanticLog log)
        {
            Guard.NotNull(webhookEventRepository, nameof(webhookEventRepository));
            Guard.NotNull(webhookRepository, nameof(webhookRepository));
            Guard.NotNull(webhookSender, nameof(webhookSender));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(log, nameof(log));

            this.webhookEventRepository = webhookEventRepository;
            this.webhookRepository = webhookRepository;
            this.webhookSender = webhookSender;

            this.clock = clock;

            this.log = log;

            requestBlock =
                new ActionBlock<IWebhookEventEntity>(MakeRequestAsync,
                    new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32, BoundedCapacity = 32 });

            blockBlock =
                new TransformBlock<IWebhookEventEntity, IWebhookEventEntity>(x => BlockAsync(x),
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
                await webhookEventRepository.QueryPendingAsync(blockBlock.SendAsync, cancellationToken);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "QueueWebhookEvents")
                    .WriteProperty("status", "Failed"));
            }
        }

        private async Task<IWebhookEventEntity> BlockAsync(IWebhookEventEntity @event)
        {
            try
            {
                await webhookEventRepository.TraceSendingAsync(@event.Id);

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

        private async Task MakeRequestAsync(IWebhookEventEntity @event)
        {
            try
            {
                var response = await webhookSender.SendAsync(@event.Job);

                Instant? nextCall = null;

                if (response.Result != WebhookResult.Success)
                {
                    var now = clock.GetCurrentInstant();

                    switch (@event.NumCalls)
                    {
                        case 0:
                            nextCall = now.Plus(Duration.FromMinutes(5));
                            break;
                        case 1:
                            nextCall = now.Plus(Duration.FromHours(1));
                            break;
                        case 2:
                            nextCall = now.Plus(Duration.FromHours(5));
                            break;
                        case 3:
                            nextCall = now.Plus(Duration.FromHours(6));
                            break;
                    }
                }

                await Task.WhenAll(
                    webhookRepository.TraceSentAsync(@event.Job.WebhookId, response.Result, response.Elapsed),
                    webhookEventRepository.TraceSentAsync(@event.Id, response.Dump, response.Result, response.Elapsed, nextCall));
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
