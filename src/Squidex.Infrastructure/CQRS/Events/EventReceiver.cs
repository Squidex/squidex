// ==========================================================================
//  EventReceiver.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Timers;

// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ConvertToLambdaExpression
// ReSharper disable MethodSupportsCancellation
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventReceiver : DisposableObjectBase
    {
        private readonly EventDataFormatter formatter;
        private readonly IEventStore eventStore;
        private readonly IEventNotifier eventNotifier;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ISemanticLog log;
        private CompletionTimer timer;

        public EventReceiver(
            EventDataFormatter formatter,
            IEventStore eventStore,
            IEventNotifier eventNotifier,
            IEventConsumerInfoRepository eventConsumerInfoRepository,
            ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventNotifier, nameof(eventNotifier));
            Guard.NotNull(eventConsumerInfoRepository, nameof(eventConsumerInfoRepository));

            this.log = log;
            this.formatter = formatter;
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    timer?.Dispose();
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, w => w
                        .WriteProperty("action", "DisposeEventReceiver")
                        .WriteProperty("state", "Failed"));
                }
            }
        }

        public void Next()
        {
            ThrowIfDisposed();

            timer?.Trigger();
        }

        public void Subscribe(IEventConsumer eventConsumer, int delay = 5000)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            ThrowIfDisposed();

            if (timer != null)
            {
                return;
            }

            var consumerName = eventConsumer.Name;
            var consumerStarted = false;

            timer = new CompletionTimer(delay, async ct =>
            {
                if (!consumerStarted)
                {
                    await eventConsumerInfoRepository.CreateAsync(consumerName);

                    consumerStarted = true;
                }

                try
                {
                    var status = await eventConsumerInfoRepository.FindAsync(consumerName);

                    var position = status.Position;

                    if (status.IsResetting)
                    {
                        position = null;

                        await ResetAsync(eventConsumer, consumerName, position);
                    }
                    else if (status.IsStopped)
                    {
                        return;
                    }

                    await eventStore.GetEventsAsync(se =>
                    {
                        return HandleEventAsync(eventConsumer, se, consumerName);
                    }, ct, eventConsumer.EventsFilter, position);
                }
                catch (Exception ex)
                {
                    log.LogFatal(ex, w => w.WriteProperty("action", "EventHandlingFailed"));

                    await eventConsumerInfoRepository.StopAsync(consumerName, ex.ToString());
                }
            });

            eventNotifier.Subscribe(timer.Trigger);
        }

        private async Task HandleEventAsync(IEventConsumer eventConsumer, StoredEvent storedEvent, string consumerName)
        {
            var @event = ParseEvent(storedEvent);

            if (@event == null)
            {
                return;
            }

            await DispatchConsumer(@event, eventConsumer, consumerName);

            await eventConsumerInfoRepository.SetPositionAsync(consumerName, storedEvent.EventPosition, false);
        }

        private async Task ResetAsync(IEventConsumer eventConsumer, string consumerName, string position)
        {
            var actionId = Guid.NewGuid().ToString();
            try
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventConsumer", eventConsumer.GetType().Name));

                await eventConsumer.ClearAsync();
                await eventConsumerInfoRepository.SetPositionAsync(consumerName, position, true);

                log.LogInformation(w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", eventConsumer.GetType().Name));
            }
            catch (Exception ex)
            {
                log.LogFatal(ex, w => w
                    .WriteProperty("action", "EventConsumerReset")
                    .WriteProperty("actionId", actionId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventConsumer", eventConsumer.GetType().Name));

                throw;
            }
        }

        private async Task DispatchConsumer(Envelope<IEvent> @event, IEventConsumer eventConsumer, string consumerName)
        {
            var eventId = @event.Headers.EventId().ToString();
            var eventType = @event.Payload.GetType().Name;
            try
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                await eventConsumer.On(@event);

                log.LogInformation(w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Completed")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("actionId", eventId)
                    .WriteProperty("state", "Started")
                    .WriteProperty("eventId", eventId)
                    .WriteProperty("eventType", eventType)
                    .WriteProperty("eventConsumer", consumerName));

                throw;
            }
        }

        private Envelope<IEvent> ParseEvent(StoredEvent storedEvent)
        {
            try
            {
                var @event = formatter.Parse(storedEvent.Data);

                @event.SetEventPosition(storedEvent.EventPosition);
                @event.SetEventStreamNumber(storedEvent.EventStreamNumber);

                return @event;
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                log.LogFatal(ex, w => w
                    .WriteProperty("action", "ParseEvent")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventId", storedEvent.Data.EventId.ToString())
                    .WriteProperty("eventPosition", storedEvent.EventPosition));

                throw;
            }
        }
    }
}