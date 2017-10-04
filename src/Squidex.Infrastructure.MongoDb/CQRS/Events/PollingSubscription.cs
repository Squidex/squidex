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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class PollingSubscription : Actor, IEventSubscription
    {
        private readonly IEventNotifier notifier;
        private readonly MongoEventStore store;
        private readonly CancellationTokenSource disposeToken = new CancellationTokenSource();
        private readonly Regex streamRegex;
        private readonly string streamFilter;
        private readonly IEventSubscriber subscriber;
        private string position;
        private bool isPolling;
        private IDisposable notification;

        private sealed class Connect
        {
        }

        private sealed class StartPoll
        {
        }

        private sealed class StopPoll
        {
        }

        public PollingSubscription(MongoEventStore store, IEventNotifier notifier, IEventSubscriber subscriber, string streamFilter, string position)
        {
            this.notifier = notifier;
            this.position = position;
            this.store = store;
            this.streamFilter = streamFilter;
            this.subscriber = subscriber;

            streamRegex = new Regex(streamFilter);

            DispatchAsync(new Connect()).Forget();
        }

        public Task StopAsync()
        {
            return StopAndWaitAsync();
        }

        protected override Task OnStop()
        {
            disposeToken?.Cancel();

            notification?.Dispose();

            return TaskHelper.Done;
        }

        protected override async Task OnError(Exception exception)
        {
            await subscriber.OnErrorAsync(this, exception);

            await StopAsync();
        }

        protected override async Task OnMessage(object message)
        {
            switch (message)
            {
                case Connect connect:
                {
                    notification = notifier.Subscribe(streamName =>
                    {
                        if (streamRegex.IsMatch(streamName))
                        {
                            DispatchAsync(new StartPoll()).Forget();
                        }
                    });

                    DispatchAsync(new StartPoll()).Forget();

                    break;
                }

                case StartPoll poll when !isPolling:
                {
                    isPolling = true;

                    PollAsync().Forget();

                    break;
                }

                case StopPoll poll when isPolling:
                {
                    isPolling = false;

                    Task.Delay(5000).ContinueWith(t => DispatchAsync(new StartPoll())).Forget();

                    break;
                }

                case StoredEvent storedEvent:
                {
                    await subscriber.OnEventAsync(this, storedEvent);

                    position = storedEvent.EventPosition;

                    break;
                }
            }
        }

        private async Task PollAsync()
        {
            try
            {
                await store.GetEventsAsync(DispatchAsync, disposeToken.Token, streamFilter, position);

                await DispatchAsync(new StopPoll());
            }
            catch (Exception ex)
            {
                if (!ex.Is<OperationCanceledException>())
                {
                    await FailAsync(ex);
                }
            }
        }
    }
}
