// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class MongoEventStoreSubscription : IEventSubscription
    {
        private readonly MongoEventStore eventStore;
        private readonly IEventSubscriber eventSubscriber;
        private readonly CancellationTokenSource stopToken = new CancellationTokenSource();

        public MongoEventStoreSubscription(MongoEventStore eventStore, IEventSubscriber eventSubscriber, string? streamFilter, string? position)
        {
            this.eventStore = eventStore;
            this.eventSubscriber = eventSubscriber;

            QueryAsync(streamFilter, position).Forget();
        }

        private async Task QueryAsync(string? streamFilter, string? position)
        {
            try
            {
                string? lastRawPosition = null;

                try
                {
                    lastRawPosition = await QueryOldAsync(streamFilter, position);
                }
                catch (OperationCanceledException)
                {
                }

                if (!stopToken.IsCancellationRequested)
                {
                    await QueryCurrentAsync(streamFilter, lastRawPosition);
                }
            }
            catch (Exception ex)
            {
                await eventSubscriber.OnErrorAsync(this, ex);
            }
        }

        private async Task QueryCurrentAsync(string? streamFilter, StreamPosition lastPosition)
        {
            BsonDocument? resumeToken = null;

            var start =
                lastPosition.Timestamp.Timestamp > 0 ?
                lastPosition.Timestamp.Timestamp - 30 :
                SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromSeconds(30)).ToUnixTimeSeconds();

            var changePipeline = Match(streamFilter);
            var changeStart = new BsonTimestamp((int)start, 0);

            while (!stopToken.IsCancellationRequested)
            {
                var changeOptions = new ChangeStreamOptions();

                if (resumeToken != null)
                {
                    changeOptions.StartAfter = resumeToken;
                }
                else
                {
                    changeOptions.StartAtOperationTime = changeStart;
                }

                using (var cursor = eventStore.TypedCollection.Watch(changePipeline, changeOptions, stopToken.Token))
                {
                    var isRead = false;

                    await cursor.ForEachAsync(async change =>
                    {
                        if (change.OperationType == ChangeStreamOperationType.Insert)
                        {
                            foreach (var storedEvent in change.FullDocument.Filtered(lastPosition))
                            {
                                await eventSubscriber.OnEventAsync(this, storedEvent);
                            }
                        }

                        isRead = true;
                    }, stopToken.Token);

                    resumeToken = cursor.GetResumeToken();

                    if (!isRead)
                    {
                        await Task.Delay(1000, stopToken.Token);
                    }
                }
            }
        }

        private async Task<string?> QueryOldAsync(string? streamFilter, string? position)
        {
            string? lastRawPosition = null;

            using (var cts = new CancellationTokenSource())
            {
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stopToken.Token))
                {
                    await foreach (var storedEvent in eventStore.QueryAllAsync(streamFilter, position, ct: combined.Token))
                    {
                        var now = SystemClock.Instance.GetCurrentInstant();

                        var timeToNow = now - storedEvent.Data.Headers.Timestamp();

                        if (timeToNow <= Duration.FromMinutes(5))
                        {
                            cts.Cancel();
                        }
                        else
                        {
                            await eventSubscriber.OnEventAsync(this, storedEvent);

                            lastRawPosition = storedEvent.EventPosition;
                        }
                    }
                }
            }

            return lastRawPosition;
        }

        private static PipelineDefinition<ChangeStreamDocument<MongoEventCommit>, ChangeStreamDocument<MongoEventCommit>>? Match(string? streamFilter)
        {
            var result = new EmptyPipelineDefinition<ChangeStreamDocument<MongoEventCommit>>();

            var byStream = FilterExtensions.ByChangeInStream(streamFilter);

            if (byStream != null)
            {
                var filterBuilder = Builders<ChangeStreamDocument<MongoEventCommit>>.Filter;

                var filter = filterBuilder.Or(filterBuilder.Ne(x => x.OperationType, ChangeStreamOperationType.Insert), byStream);

                return result.Match(filter);
            }

            return result;
        }

        public void Unsubscribe()
        {
            stopToken.Cancel();
        }
    }
}
