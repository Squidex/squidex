// ==========================================================================
//  GetEventStoreSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.CQRS.Events
{
    internal sealed class EventStoreSubscription : DisposableObjectBase, IEventSubscription
    {
        private const int ReconnectWindowMax = 5;
        private const int ReconnectWaitMs = 1000;
        private static readonly TimeSpan TimeBetweenReconnects = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, bool> subscriptionsCreated = new ConcurrentDictionary<string, bool>();
        private readonly IEventStoreConnection connection;
        private readonly string streamFilter;
        private readonly string streamName;
        private readonly string prefix;
        private readonly string projectionHost;
        private readonly ReaderWriterLockSlim connectionLock = new ReaderWriterLockSlim();
        private readonly Queue<DateTime> reconnectTimes = new Queue<DateTime>();
        private readonly CancellationTokenSource disposeToken = new CancellationTokenSource();
        private Func<StoredEvent, Task> publishNext;
        private Func<Exception, Task> publishError;
        private EventStoreCatchUpSubscription internalSubscription;
        private long? position;

        public EventStoreSubscription(IEventStoreConnection connection, string streamFilter, string position, string prefix, string projectionHost)
        {
            this.prefix = prefix;
            this.position = ParsePosition(position);
            this.connection = connection;
            this.streamFilter = streamFilter;
            this.projectionHost = projectionHost;

            streamName = $"by-{prefix.Simplify()}-{streamFilter.Simplify()}";
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                disposeToken.Cancel();

                try
                {
                    connectionLock.EnterWriteLock();

                    internalSubscription?.Stop();
                    internalSubscription = null;
                }
                finally
                {
                    connectionLock.ExitWriteLock();
                }
            }
        }

        public async Task SubscribeAsync(Func<StoredEvent, Task> onNext, Func<Exception, Task> onError = null)
        {
            Guard.NotNull(onNext, nameof(onNext));

            if (publishNext != null)
            {
                throw new InvalidOperationException("An handler has already been registered.");
            }

            publishNext = onNext;
            publishError = onError;

            await CreateProjectionAsync();

            try
            {
                connectionLock.EnterWriteLock();

                internalSubscription = SubscribeToEventStore();
            }
            finally
            {
                connectionLock.ExitWriteLock();
            }
        }

        private EventStoreCatchUpSubscription SubscribeToEventStore()
        {
            return connection.SubscribeToStreamFrom(streamName, position, CatchUpSubscriptionSettings.Default, HandleEvent, null, HandleError);
        }

        private void HandleEvent(EventStoreCatchUpSubscription subscription, ResolvedEvent resolved)
        {
            if (!CanHandleSubscriptionEvent(subscription))
            {
                return;
            }

            try
            {
                connectionLock.EnterReadLock();

                if (CanHandleSubscriptionEvent(subscription))
                {
                    var storedEvent = Formatter.Read(resolved);

                    PublishAsync(storedEvent).Wait();

                    position = resolved.OriginalEventNumber;
                }
            }
            finally
            {
                connectionLock.ExitReadLock();
            }
        }

        private void HandleError(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception ex)
        {
            if (!CanHandleSubscriptionEvent(subscription))
            {
                return;
            }

            try
            {
                connectionLock.EnterUpgradeableReadLock();

                if (CanHandleSubscriptionEvent(subscription))
                {
                    if (reason == SubscriptionDropReason.ConnectionClosed)
                    {
                        var utcNow = DateTime.UtcNow;

                        if (CanReconnect(utcNow))
                        {
                            RegisterReconnectTime(utcNow);

                            try
                            {
                                connectionLock.EnterWriteLock();

                                internalSubscription.Stop();
                                internalSubscription = null;

                                internalSubscription = SubscribeToEventStore();
                            }
                            finally
                            {
                                connectionLock.ExitWriteLock();
                            }

                            DelayForReconnect().Wait();

                            if (!CanHandleSubscriptionEvent(subscription))
                            {
                                return;
                            }

                            try
                            {
                                connectionLock.EnterWriteLock();

                                if (CanHandleSubscriptionEvent(subscription))
                                {
                                    internalSubscription = SubscribeToEventStore();
                                }
                            }
                            finally
                            {
                                connectionLock.ExitWriteLock();
                            }

                            return;
                        }
                    }

                    if (reason != SubscriptionDropReason.UserInitiated)
                    {
                        var exception = ex ?? new ConnectionClosedException($"Subscription closed with reason {reason}.");

                        publishError?.Invoke(exception);
                    }
                }
            }
            finally
            {
                connectionLock.ExitUpgradeableReadLock();
            }
        }

        private bool CanHandleSubscriptionEvent(EventStoreCatchUpSubscription subscription)
        {
            return !disposeToken.IsCancellationRequested && subscription == internalSubscription;
        }

        private bool CanReconnect(DateTime utcNow)
        {
            return reconnectTimes.Count < ReconnectWindowMax && (reconnectTimes.Count == 0 || (utcNow - reconnectTimes.Peek()) > TimeBetweenReconnects);
        }

        private async Task PublishAsync(StoredEvent storedEvent)
        {
            await publishNext(storedEvent).ConfigureAwait(false);
        }

        private static long? ParsePosition(string position)
        {
            return long.TryParse(position, out var parsedPosition) ? (long?)parsedPosition : null;
        }

        private void RegisterReconnectTime(DateTime utcNow)
        {
            reconnectTimes.Enqueue(utcNow);

            while (reconnectTimes.Count >= ReconnectWindowMax)
            {
                reconnectTimes.Dequeue();
            }
        }

        private async Task DelayForReconnect()
        {
            try
            {
                await Task.Delay(ReconnectWaitMs, disposeToken.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Just ignore.
            }
        }

        private async Task CreateProjectionAsync()
        {
            if (subscriptionsCreated.TryAdd(streamName, true))
            {
                var projectsManager = await ConnectToProjections();

                var projectionConfig =
                    $@"fromAll()
                        .when({{
                            $any: function (s, e) {{
                                if (e.streamId.indexOf('{prefix}') === 0 && /{streamFilter}/.test(e.streamId.substring({prefix.Length + 1}))) {{
                                    linkTo('{streamName}', e);
                                }}
                            }}
                        }});";

                try
                {
                    await projectsManager.CreateContinuousAsync($"${streamName}", projectionConfig, connection.Settings.DefaultUserCredentials);
                }
                catch (ProjectionCommandConflictException)
                {
                    // Projection already exists.
                }
            }
        }

        private async Task<ProjectionsManager> ConnectToProjections()
        {
            var addressParts = projectionHost.Split(':');

            if (addressParts.Length < 2 || !int.TryParse(addressParts[1], out var port))
            {
                port = 2113;
            }

            var endpoints = await Dns.GetHostAddressesAsync(addressParts[0]);
            var endpoint = new IPEndPoint(endpoints.First(x => x.AddressFamily == AddressFamily.InterNetwork), port);

            var projectionsManager =
                new ProjectionsManager(
                    connection.Settings.Log, endpoint,
                    connection.Settings.OperationTimeout);

            return projectionsManager;
        }
    }
}
