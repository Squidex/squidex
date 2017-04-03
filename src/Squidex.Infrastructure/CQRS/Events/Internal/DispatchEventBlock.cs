// ==========================================================================
//  DispatchEventBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    internal sealed class DispatchEventBlock : EventReceiverBlock<Envelope<IEvent>, Envelope<IEvent>>
    {
        public DispatchEventBlock(IEventConsumer eventConsumer, IEventConsumerInfoRepository eventConsumerInfoRepository, ISemanticLog log) 
            : base(true, eventConsumer, eventConsumerInfoRepository, log)
        {
        }

        protected override async Task<Envelope<IEvent>> On(Envelope<IEvent> input)
        {
            var consumerName = EventConsumer.Name;

            var eventId = input.Headers.EventId().ToString();
            var eventType = input.Payload.GetType().Name;
            try
            {
                Log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                await EventConsumer.On(input);

                Log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                return input;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                throw;
            }
        }

        protected override long GetEventNumber(Envelope<IEvent> input)
        {
            return input.Headers.EventNumber();
        }
    }
}
