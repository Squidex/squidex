// ==========================================================================
//  DispatchEventBlock.cs
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
    internal sealed class DispatchEventBlock : IEventReceiverBlock
    {
        private readonly ISemanticLog log;
        private readonly IEventConsumer eventConsumer;
        private readonly TransformBlock<Envelope<IEvent>, Envelope<IEvent>> transformBlock;
        private long lastReceivedEventNumber = -1;
        private bool isRunning = true;

        public Action<Exception> OnError { get; set; }

        public ITargetBlock<Envelope<IEvent>> Target
        {
            get { return transformBlock; }
        }

        public DispatchEventBlock(IEventConsumer eventConsumer, ISemanticLog log) 
        {
            this.eventConsumer = eventConsumer;

            this.log = log;

            var nullHandler = new ActionBlock<Envelope<IEvent>>(x => { });

            transformBlock = 
                new TransformBlock<Envelope<IEvent>, Envelope<IEvent>>(x => HandleAsync(x),
                    new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });
            transformBlock.LinkTo(nullHandler, x => x == null);
        }

        public void LinkTo(ITargetBlock<Envelope<IEvent>> target)
        {
            transformBlock.LinkTo(target, new DataflowLinkOptions { PropagateCompletion = true });
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

        private async Task<Envelope<IEvent>> HandleAsync(Envelope<IEvent> input)
        {
            var eventNumber = input.Headers.EventNumber();

            if (eventNumber <= lastReceivedEventNumber || !isRunning)
            {
                return null;
            }

            var consumerName = eventConsumer.Name;

            var eventId = input.Headers.EventId().ToString();
            var eventType = input.Payload.GetType().Name;
            try
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                await eventConsumer.On(input);

                log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                lastReceivedEventNumber = eventNumber;

                return input;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);

                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                return null;
            }
        }
    }
}
