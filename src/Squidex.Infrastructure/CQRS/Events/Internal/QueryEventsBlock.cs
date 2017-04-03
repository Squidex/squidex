// ==========================================================================
//  QueryEventsBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    internal sealed class QueryEventsBlock : IEventReceiverBlock
    {
        private readonly ISemanticLog log;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly IEventConsumer eventConsumer;
        private readonly IEventStore eventStore;
        private readonly ActionBlock<Envelope<IEvent>> actionBlock;
        private bool isRunning = true;
        private bool isStarted;

        public Action<Exception> OnError { get; set; }

        public Action OnReset { get; set; }

        public Func<StoredEvent, Task> OnEvent { get; set; }

        public ITargetBlock<Envelope<IEvent>> Target
        {
            get { return actionBlock; }
        }

        public Task Completion
        {
            get { return actionBlock.Completion; }
        }

        public QueryEventsBlock(IEventConsumerInfoRepository eventConsumerInfoRepository, IEventConsumer eventConsumer, IEventStore eventStore, ISemanticLog log)
        {
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;
            this.eventConsumer = eventConsumer;
            this.eventStore = eventStore;

            this.log = log;

            actionBlock =
                new ActionBlock<Envelope<IEvent>>(HandleAsync,
                    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
        }

        public void Complete()
        {
            actionBlock.Complete();
        }

        public void NextOrThrowAway()
        {
            actionBlock.Post(null);
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Reset()
        {
            isRunning = true;
        }

        private async Task HandleAsync(object input)
        {
            try
            {
                if (!isStarted)
                {
                    await eventConsumerInfoRepository.CreateAsync(eventConsumer.Name);

                    isStarted = true;
                }

                var status = await eventConsumerInfoRepository.FindAsync(eventConsumer.Name);

                var lastReceivedEventNumber = status.LastHandledEventNumber;

                if (status.IsResetting)
                {
                    await ResetAsync();

                    Reset();
                }

                if (!status.IsStopped || !isRunning)
                {
                    var ct = new CancellationTokenSource();

                    await eventStore.GetEventsAsync(async storedEvent =>
                    {
                        if (!isRunning)
                        {
                            ct.Cancel();
                        }

                        var onEvent = OnEvent;

                        if (onEvent != null)
                        {
                            await onEvent(storedEvent);
                        }
                    }, ct.Token, null, lastReceivedEventNumber);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        private async Task ResetAsync()
        {
            var consumerName = eventConsumer.Name;

            var actionId = Guid.NewGuid().ToString();
            try
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventConsumer", consumerName));

                await eventConsumer.ClearAsync();
                await eventConsumerInfoRepository.SetLastHandledEventNumberAsync(consumerName, -1);

                log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", consumerName));

                OnReset?.Invoke();
            }
            catch (Exception ex)
            {
                log.LogFatal(ex, w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", consumerName));
            }
        }
    }
}
