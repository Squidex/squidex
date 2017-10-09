// ==========================================================================
//  EventConsumerActor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events.Actors
{
    public sealed class EventConsumerActor : DisposableObjectBase, IEventSubscriber, IActor
    {
        private readonly EventDataFormatter formatter;
        private readonly RetryWindow retryWindow = new RetryWindow(TimeSpan.FromMinutes(5), 5);
        private readonly IEventStore eventStore;
        private readonly IEventConsumerInfoRepository eventConsumerInfoRepository;
        private readonly ISemanticLog log;
        private readonly ActionBlock<object> dispatcher;
        private IEventSubscription eventSubscription;
        private IEventConsumer eventConsumer;
        private bool isStopped;
        private bool statusIsRunning = true;
        private string statusPosition;
        private string statusError;
        private Guid stateId = Guid.NewGuid();

        private sealed class Teardown
        {
        }

        private sealed class Setup
        {
            public IEventConsumer EventConsumer { get; set; }
        }

        private abstract class SubscriptionMessage
        {
            public IEventSubscription Subscription { get; set; }
        }

        private sealed class SubscriptionEventReceived : SubscriptionMessage
        {
            public StoredEvent Event { get; set; }
        }

        private sealed class SubscriptionFailed : SubscriptionMessage
        {
            public Exception Exception { get; set; }
        }

        private sealed class Reconnect
        {
            public Guid StateId { get; set; }
        }

        public int ReconnectWaitMs { get; set; } = 5000;

        public EventConsumerActor(
            EventDataFormatter formatter,
            IEventStore eventStore,
            IEventConsumerInfoRepository eventConsumerInfoRepository,
            ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventConsumerInfoRepository, nameof(eventConsumerInfoRepository));

            this.log = log;

            this.formatter = formatter;
            this.eventStore = eventStore;
            this.eventConsumerInfoRepository = eventConsumerInfoRepository;

            var options = new ExecutionDataflowBlockOptions
            {
                MaxMessagesPerTask = -1,
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 10
            };

            dispatcher = new ActionBlock<object>(OnMessage, options);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                dispatcher.SendAsync(new Teardown()).Wait();
                dispatcher.Complete();
                dispatcher.Completion.Wait();
            }
        }

        public async Task WaitForCompletionAsync()
        {
            while (dispatcher.InputCount > 0)
            {
                await Task.Delay(20);
            }
        }

        public Task SubscribeAsync(IEventConsumer eventConsumer)
        {
            Guard.NotNull(eventConsumer, nameof(eventConsumer));

            return dispatcher.SendAsync(new Setup { EventConsumer = eventConsumer });
        }

        Task IEventSubscriber.OnEventAsync(IEventSubscription subscription, StoredEvent @event)
        {
            return dispatcher.SendAsync(new SubscriptionEventReceived { Subscription = subscription, Event = @event });
        }

        Task IEventSubscriber.OnErrorAsync(IEventSubscription subscription, Exception exception)
        {
            return dispatcher.SendAsync(new SubscriptionFailed { Subscription = subscription, Exception = exception });
        }

        void IActor.Tell(object message)
        {
            dispatcher.SendAsync(message).Forget();
        }

        private async Task OnMessage(object message)
        {
            if (isStopped)
            {
                return;
            }

            try
            {
                var oldStateId = stateId;
                var newStateId = stateId = Guid.NewGuid();

                switch (message)
                {
                    case Teardown teardown:
                    {
                        isStopped = true;

                        return;
                    }

                    case Setup setup:
                    {
                        eventConsumer = setup.EventConsumer;

                        var status = await eventConsumerInfoRepository.FindAsync(eventConsumer.Name);

                        if (status != null)
                        {
                            statusError = status.Error;
                            statusPosition = status.Position;
                            statusIsRunning = !status.IsStopped;
                        }

                        if (statusIsRunning)
                        {
                            await SubscribeThisAsync(statusPosition);
                        }

                        break;
                    }

                    case StartConsumerMessage startConsumer:
                    {
                        if (statusIsRunning)
                        {
                            return;
                        }

                        await SubscribeThisAsync(statusPosition);

                        statusError = null;
                        statusIsRunning = true;

                        break;
                    }

                    case StopConsumerMessage stopConsumer:
                    {
                        if (!statusIsRunning)
                        {
                            return;
                        }

                        await UnsubscribeThisAsync();

                        statusIsRunning = false;

                        break;
                    }

                    case ResetConsumerMessage resetConsumer:
                    {
                        await UnsubscribeThisAsync();
                        await ClearAsync();
                        await SubscribeThisAsync(null);

                        statusError = null;
                        statusPosition = null;
                        statusIsRunning = true;

                        break;
                    }

                    case Reconnect reconnect:
                    {
                        if (!statusIsRunning || reconnect.StateId != oldStateId)
                        {
                            return;
                        }

                        await SubscribeThisAsync(statusPosition);

                        break;
                    }

                    case SubscriptionFailed subscriptionFailed:
                    {
                        if (subscriptionFailed.Subscription != eventSubscription)
                        {
                            return;
                        }

                        await UnsubscribeThisAsync();

                        if (retryWindow.CanRetryAfterFailure())
                        {
                            Task.Delay(ReconnectWaitMs).ContinueWith(t => dispatcher.SendAsync(new Reconnect { StateId = newStateId })).Forget();
                        }
                        else
                        {
                            throw subscriptionFailed.Exception;
                        }

                        break;
                    }

                    case SubscriptionEventReceived eventReceived:
                    {
                        if (eventReceived.Subscription != eventSubscription)
                        {
                            return;
                        }

                        var @event = ParseEvent(eventReceived.Event);

                        await DispatchConsumerAsync(@event);

                        statusError = null;
                        statusPosition = @eventReceived.Event.EventPosition;

                        break;
                    }
                }

                await eventConsumerInfoRepository.SetAsync(eventConsumer.Name, statusPosition, !statusIsRunning, statusError);
            }
            catch (Exception ex)
            {
                try
                {
                    await UnsubscribeThisAsync();
                }
                catch (Exception unsubscribeException)
                {
                    ex = new AggregateException(ex, unsubscribeException);
                }

                log.LogFatal(ex, w => w
                    .WriteProperty("action", "HandleEvent")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("eventConsumer", eventConsumer.Name));

                statusError = ex.ToString();
                statusIsRunning = false;

                await eventConsumerInfoRepository.SetAsync(eventConsumer.Name, statusPosition, !statusIsRunning, statusError);
            }
        }

        private async Task UnsubscribeThisAsync()
        {
            if (eventSubscription != null)
            {
                await eventSubscription.StopAsync();

                eventSubscription = null;
            }
        }

        private Task SubscribeThisAsync(string position)
        {
            if (eventSubscription == null)
            {
                eventSubscription = eventStore.CreateSubscription(this, eventConsumer.EventsFilter, position);
            }

            return TaskHelper.Done;
        }

        private async Task ClearAsync()
        {
            var actionId = Guid.NewGuid().ToString();

            log.LogInformation(w => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", actionId)
                .WriteProperty("state", "Started")
                .WriteProperty("eventConsumer", eventConsumer.Name));

            using (log.MeasureTrace(w => w
                .WriteProperty("action", "EventConsumerReset")
                .WriteProperty("actionId", actionId)
                .WriteProperty("state", "Completed")
                .WriteProperty("eventConsumer", eventConsumer.Name)))
            {
                await eventConsumer.ClearAsync();
            }
        }

        private async Task DispatchConsumerAsync(Envelope<IEvent> @event)
        {
            var eventId = @event.Headers.EventId().ToString();
            var eventType = @event.Payload.GetType().Name;

            log.LogInformation(w => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("actionId", eventId)
                .WriteProperty("state", "Started")
                .WriteProperty("eventId", eventId)
                .WriteProperty("eventType", eventType)
                .WriteProperty("eventConsumer", eventConsumer.Name));

            using (log.MeasureTrace(w => w
                .WriteProperty("action", "HandleEvent")
                .WriteProperty("actionId", eventId)
                .WriteProperty("state", "Completed")
                .WriteProperty("eventId", eventId)
                .WriteProperty("eventType", eventType)
                .WriteProperty("eventConsumer", eventConsumer.Name)))
            {
                await eventConsumer.On(@event);
            }
        }

        private Envelope<IEvent> ParseEvent(StoredEvent message)
        {
            var @event = formatter.Parse(message.Data);

            @event.SetEventPosition(message.EventPosition);
            @event.SetEventStreamNumber(message.EventStreamNumber);

            return @event;
        }
    }
}