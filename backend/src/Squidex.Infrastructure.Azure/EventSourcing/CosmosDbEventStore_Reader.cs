// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Hosting;
using Squidex.Log;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate bool EventPredicate(EventData data);

    public partial class CosmosDbEventStore : IEventStore, IInitializable
    {
        private static readonly IReadOnlyList<StoredEvent> EmptyEvents = new List<StoredEvent>();

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string? streamFilter = null, string? position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));

            ThrowIfDisposed();

            return new CosmosDbSubscription(this, subscriber, streamFilter, position);
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryLatestAsync(string streamName, int count)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            ThrowIfDisposed();

            if (count <= 0)
            {
                return EmptyEvents;
            }

            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                var query = FilterBuilder.ByStreamNameDesc(streamName, count);

                var result = new List<StoredEvent>();

                await foreach (var commit in documentClient.QueryAsync(collectionUri, query, default))
                {
                    foreach (var storedEvent in commit.Filtered().Reverse())
                    {
                        result.Add(storedEvent);

                        if (result.Count == count)
                        {
                            break;
                        }
                    }
                }

                return result;
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            ThrowIfDisposed();

            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                var query = FilterBuilder.ByStreamName(streamName, streamPosition - MaxCommitSize);

                var result = new List<StoredEvent>();

                await foreach (var commit in documentClient.QueryAsync(collectionUri, query, default))
                {
                    foreach (var storedEvent in commit.Filtered().Reverse())
                    {
                        result.Add(storedEvent);
                    }
                }

                return result;
            }
        }

        public async IAsyncEnumerable<StoredEvent> QueryAllAsync( string? streamFilter = null, string? position = null, long take = long.MaxValue,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            ThrowIfDisposed();

            if (take <= 0)
            {
                yield break;
            }

            StreamPosition lastPosition = position;

            var filterDefinition = FilterBuilder.CreateByFilter(streamFilter, lastPosition, "ASC", take);

            var taken = int.MaxValue;

            await foreach (var commit in documentClient.QueryAsync(collectionUri, filterDefinition, ct: ct))
            {
                if (taken == take)
                {
                    yield break;
                }

                foreach (var storedEvent in commit.Filtered(lastPosition))
                {
                    if (taken == take)
                    {
                        yield break;
                    }

                    yield return storedEvent;

                    taken++;
                }
            }
        }

        public async IAsyncEnumerable<StoredEvent> QueryAllReverseAsync(string? streamFilter = null, string? position = null, long take = long.MaxValue,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            ThrowIfDisposed();

            if (take <= 0)
            {
                yield break;
            }

            StreamPosition lastPosition = position;

            var filterDefinition = FilterBuilder.CreateByFilter(streamFilter, lastPosition, "DESC", take);

            var taken = long.MaxValue;

            await foreach (var commit in documentClient.QueryAsync(collectionUri, filterDefinition, ct: ct))
            {
                if (taken == take)
                {
                    yield break;
                }

                foreach (var storedEvent in commit.Filtered(lastPosition))
                {
                    if (taken == take)
                    {
                        yield break;
                    }

                    yield return storedEvent;

                    taken++;
                }
            }
        }
    }
}