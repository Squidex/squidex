// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NodaTime;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventReceiver
    {
        private readonly EventDataFormatter formatter;
        private readonly IEnumerable<ILiveEventConsumer> liveConsumers;
        private readonly IEnumerable<ICatchEventConsumer> catchConsumers;
        private readonly IEventStream eventStream;
        private readonly ILogger<EventReceiver> logger;
        private bool isSubscribed;

        public EventReceiver(
            ILogger<EventReceiver> logger,
            IEventStream eventStream,
            IEnumerable<ILiveEventConsumer> liveConsumers,
            IEnumerable<ICatchEventConsumer> catchConsumers,
            EventDataFormatter formatter)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStream, nameof(eventStream));
            Guard.NotNull(liveConsumers, nameof(liveConsumers));
            Guard.NotNull(catchConsumers, nameof(catchConsumers));

            this.logger = logger;
            this.formatter = formatter;
            this.eventStream = eventStream;
            this.liveConsumers = liveConsumers;
            this.catchConsumers = catchConsumers;
        }

        public void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            var startTime = SystemClock.Instance.GetCurrentInstant();

            eventStream.Connect("squidex", eventData =>
            {
                var @event = ParseEvent(eventData);

                if (@event == null)
                {
                    return;
                }

                var isLive = @event.Headers.Timestamp() >= startTime;

                if (isLive)
                {
                    DispatchConsumers(catchConsumers.OfType<IEventConsumer>().Union(liveConsumers), @event);
                }
                else
                {
                    DispatchConsumers(catchConsumers, @event);
                }
            });

            isSubscribed = true;
        }

        private void DispatchConsumers(IEnumerable<IEventConsumer> consumers, Envelope<IEvent> @event)
        {
            Task.WaitAll(consumers.Select(c => DispatchConsumer(@event, c)).ToArray());
        }

        private async Task DispatchConsumer(Envelope<IEvent> @event, IEventConsumer consumer)
        {
            try
            {
                await consumer.On(@event);
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventHandlingFailed, ex, "[{0}]: Failed to handle event {1} ({2})", consumer, @event.Payload, @event.Headers.EventId());
            }
        }

        private Envelope<IEvent> ParseEvent(EventData eventData)
        {
            try
            {
                var @event = formatter.Parse(eventData);

                return @event;
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.EventDeserializationFailed, ex, "Failed to parse event {0}", eventData.EventId);

                return null;
            }
        }
    }
}