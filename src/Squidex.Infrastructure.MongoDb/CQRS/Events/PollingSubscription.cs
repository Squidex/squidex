// ==========================================================================
//  PollingSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Tasks;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class PollingSubscription : Actor, IEventSubscription
    {
        private readonly IEventNotifier eventNotifier;
        private readonly MongoEventStore eventStore;
        private CancellationTokenSource cancelPolling;
        private Timer pollTimer;
        private Regex streamRegex;
        private Guid subscription;
        private string streamFilter;
        private string position;
        private IDisposable pollSubscription;
        private IActor parent;

        private sealed class PollMessage : IMessage
        {
        }

        private sealed class ReceiveMongoEventMessage : IMessage
        {
            public StoredEvent Event;

            public Guid Subscription;
        }

        public PollingSubscription(MongoEventStore eventStore, IEventNotifier eventNotifier)
        {
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
        }

        protected override Task OnStop()
        {
            cancelPolling?.Cancel();

            pollTimer?.Dispose();
            pollSubscription?.Dispose();

            parent = null;

            return TaskHelper.Done;
        }

        protected override async Task OnError(Exception exception)
        {
            if (parent != null)
            {
                await parent.SendAsync(exception);
            }

            await StopAsync();
        }

        protected override async Task OnMessage(IMessage message)
        {
            switch (message)
            {
                case SubscribeMessage subscribe when parent == null:
                    {
                        parent = subscribe.Parent;
                        position = subscribe.Position;

                        streamFilter = subscribe.StreamFilter;
                        streamRegex = new Regex(streamFilter);

                        pollSubscription = eventNotifier.Subscribe(streamName =>
                        {
                            if (streamRegex.IsMatch(streamName))
                            {
                                SendAsync(new PollMessage()).Forget();
                            }
                        });

                        pollTimer = new Timer(d =>
                        {
                            SendAsync(new PollMessage()).Forget();
                        });

                        pollTimer.Change(0, 5000);

                        break;
                    }

                case PollMessage poll when parent != null:
                    {
                        cancelPolling?.Cancel();
                        cancelPolling = new CancellationTokenSource();

                        subscription = Guid.NewGuid();

                        PollAsync(subscription, cancelPolling.Token).Forget();

                        break;
                    }

                case ReceiveMongoEventMessage receiveEvent when parent != null:
                    {
                        if (receiveEvent.Subscription == subscription)
                        {
                            await parent.SendAsync(new ReceiveEventMessage { Event = receiveEvent.Event, Source = this });

                            position = receiveEvent.Event.EventPosition;
                        }

                        break;
                    }
            }
        }

        private async Task PollAsync(Guid subscriptionId, CancellationToken ct)
        {
            try
            {
                await eventStore.GetEventsAsync(async e =>
                {
                    if (ct.IsCancellationRequested == true)
                    {
                        await SendAsync(new ReceiveMongoEventMessage { Event = e, Subscription = subscriptionId });
                    }
                }, ct, streamFilter, position);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                await SendAsync(ex);
            }
        }
    }
}
