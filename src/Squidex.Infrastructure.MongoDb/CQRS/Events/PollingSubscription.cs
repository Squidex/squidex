// ==========================================================================
//  PollingSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class PollingSubscription : Actor, IEventSubscription
    {
        private readonly IEventNotifier eventNotifier;
        private readonly MongoEventStore eventStore;
        private readonly string streamFilter;
        private CancellationTokenSource ct;
        private Timer pollTimer;
        private string position;
        private bool isStopped;
        private IDisposable pollSubscription;
        private IActor parent;

        private sealed class PollMessage : IMessage
        {
        }

        public PollingSubscription(MongoEventStore eventStore, IEventNotifier eventNotifier, string streamFilter, string position)
        {
            this.position = position;
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
            this.streamFilter = streamFilter;
        }

        protected override Task OnStop()
        {
            ct?.Cancel();

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

                        pollSubscription = eventNotifier.Subscribe(() =>
                        {
                            SendAsync(new PollMessage()).Forget();
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
                        ct?.Cancel();
                        ct = new CancellationTokenSource();

                        PollAsync().Forget();

                        break;
                    }

                case ReceiveEventMessage receiveEvent when parent != null:
                    {
                        await parent.SendAsync(receiveEvent);

                        position = receiveEvent.Event.EventPosition;

                        break;
                    }
            }
        }

        private async Task PollAsync()
        {
            try
            {
                await eventStore.GetEventsAsync(e => SendAsync(new ReceiveEventMessage { Event = e }), ct.Token, streamFilter, position);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                await SendAsync(ex);
            }
        }
    }
}
