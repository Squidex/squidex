// ==========================================================================
//  UpdateStateBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    internal sealed class UpdateStateBlock : IEventReceiverBlock
    {
        private readonly ISemanticLog log;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly IEventConsumer eventConsumer;
        private readonly ActionBlock<Envelope<IEvent>> actionBlock;
        private long lastReceivedEventNumber = -1;
        private bool isRunning = true;

        public Action<Exception> OnError { get; set; }

        public ITargetBlock<Envelope<IEvent>> Target
        {
            get { return actionBlock; }
        }

        public Task Completion
        {
            get { return actionBlock.Completion; }
        }

        public UpdateStateBlock(IEventConsumerInfoRepository eventConsumerInfoRepository, IEventConsumer eventConsumer, ISemanticLog log)
        {
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;
            this.eventConsumer = eventConsumer;

            this.log = log;

            actionBlock =
                new ActionBlock<Envelope<IEvent>>(HandleAsync,
                    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Reset()
        {
            isRunning = true;

            lastReceivedEventNumber = -1;
        }

        private async Task HandleAsync(Envelope<IEvent> input)
        {
            var eventNumber = input.Headers.EventNumber();

            if (eventNumber <= lastReceivedEventNumber || !isRunning)
            {
                return;
            }

            try
            {
                await eventConsumerInfoRepository.SetLastHandledEventNumberAsync(eventConsumer.Name, eventNumber);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);

                log.LogFatal(ex, w => w
                    .WriteProperty("action", "UpdateState")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventId", input.Headers.EventId().ToString())
                    .WriteProperty("eventNumber", input.Headers.EventNumber()));
            }
        }
    }
}
