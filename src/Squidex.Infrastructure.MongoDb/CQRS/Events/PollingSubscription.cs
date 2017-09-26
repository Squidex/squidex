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
        private readonly CancellationTokenSource pollStop = new CancellationTokenSource();
        private Regex streamRegex;
        private string streamFilter;
        private string position;
        private bool isPolling;
        private IDisposable pollSubscription;
        private IActor parent;

        private sealed class StartPollMessage : IMessage
        {
        }

        private sealed class StopPollMessage : IMessage
        {
        }

        public PollingSubscription(MongoEventStore eventStore, IEventNotifier eventNotifier)
        {
            this.eventStore = eventStore;
            this.eventNotifier = eventNotifier;
        }

        protected override Task OnStop()
        {
            pollStop?.Cancel();
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
                                SendAsync(new StartPollMessage()).Forget();
                            }
                        });

                        SendAsync(new StartPollMessage()).Forget();

                        break;
                    }

                case StartPollMessage poll when parent != null:
                    {
                        if (!isPolling)
                        {
                            isPolling = true;

                            PollAsync().Forget();
                        }

                        break;
                    }

                case StopPollMessage poll when parent != null:
                    {
                        isPolling = false;

                        Task.Delay(5000).ContinueWith(t => SendAsync(new StartPollMessage())).Forget();

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
                await eventStore.GetEventsAsync(e => SendAsync(new ReceiveEventMessage { Event = e, Source = this }), pollStop.Token, streamFilter, position);

                await SendAsync(new StopPollMessage());
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                await SendAsync(ex);
            }
        }
    }
}
