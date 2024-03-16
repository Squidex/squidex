// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;
using Builder = Microsoft.Azure.Documents.ChangeFeedProcessor.ChangeFeedProcessorBuilder;
using Collection = Microsoft.Azure.Documents.ChangeFeedProcessor.DocumentCollectionInfo;
using Options = Microsoft.Azure.Documents.ChangeFeedProcessor.ChangeFeedProcessorOptions;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class CosmosDbSubscription : IEventSubscription, IChangeFeedObserverFactory, IChangeFeedObserver
    {
        private readonly TaskCompletionSource<bool> processorStopRequested = new TaskCompletionSource<bool>();
        private readonly CosmosDbEventStore store;
        private readonly Regex regex;
        private readonly string hostName;
        private readonly IEventSubscriber subscriber;

        public CosmosDbSubscription(CosmosDbEventStore store, IEventSubscriber subscriber, string? streamFilter, string? position = null)
        {
            this.store = store;

            var fromBeginning = string.IsNullOrWhiteSpace(position);

            if (fromBeginning)
            {
                hostName = $"squidex.{DateTime.UtcNow.Ticks}";
            }
            else
            {
                hostName = position ?? "none";
            }

            if (!StreamFilter.IsAll(streamFilter))
            {
                regex = new Regex(streamFilter);
            }

            this.subscriber = subscriber;

            Task.Run(async () =>
            {
                try
                {
                    var processor =
                        await new Builder()
                            .WithFeedCollection(CreateCollection(store, Constants.Collection))
                            .WithLeaseCollection(CreateCollection(store, Constants.LeaseCollection))
                            .WithHostName(hostName)
                            .WithProcessorOptions(new Options { StartFromBeginning = fromBeginning, LeasePrefix = hostName })
                            .WithObserverFactory(this)
                            .BuildAsync();

                    await processor.StartAsync();
                    await processorStopRequested.Task;
                    await processor.StopAsync();
                }
                catch (Exception ex)
                {
                    await subscriber.OnErrorAsync(this, ex);
                }
            });
        }

        private static Collection CreateCollection(CosmosDbEventStore store, string name)
        {
            var collection = new Collection();

            collection.CollectionName = name;
            collection.DatabaseName = store.DatabaseId;
            collection.MasterKey = store.MasterKey;
            collection.Uri = store.ServiceUri;

            return collection;
        }

        public IChangeFeedObserver CreateObserver()
        {
            return this;
        }

        public async Task CloseAsync(IChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            if (reason == ChangeFeedObserverCloseReason.ObserverError)
            {
                await subscriber.OnErrorAsync(this, new InvalidOperationException("Change feed observer failed."));
            }
        }

        public Task OpenAsync(IChangeFeedObserverContext context)
        {
            return Task.CompletedTask;
        }

        public async Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs,
            CancellationToken cancellationToken)
        {
            if (!processorStopRequested.Task.IsCompleted)
            {
                foreach (var document in docs)
                {
                    if (!processorStopRequested.Task.IsCompleted)
                    {
                        var streamName = document.GetPropertyValue<string>("eventStream");

                        if (regex == null || regex.IsMatch(streamName))
                        {
                            var commit = store.JsonSerializer.Deserialize<CosmosDbEventCommit>(document.ToString());

                            var eventStreamOffset = (int)commit.EventStreamOffset;

                            foreach (var @event in commit.Events)
                            {
                                eventStreamOffset++;

                                var eventData = @event.ToEventData();

                                await subscriber.OnEventAsync(this, new StoredEvent(commit.EventStream, hostName, eventStreamOffset, eventData));
                            }
                        }
                    }
                }
            }
        }

        public void Unsubscribe()
        {
            processorStopRequested.SetResult(true);
        }
    }
}
