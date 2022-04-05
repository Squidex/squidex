// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NodaTime;
using Squidex.Log;

namespace Squidex.Infrastructure.EventSourcing
{
    public partial class CosmosDbEventStore
    {
        private const int MaxWriteAttempts = 20;
        private const int MaxCommitSize = 10;

        public async Task DeleteStreamAsync(string streamName)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            ThrowIfDisposed();

            var query = FilterBuilder.AllIds(streamName);

            var deleteOptions = new RequestOptions
            {
                PartitionKey = new PartitionKey(streamName)
            };

            await foreach (var commit in documentClient.QueryAsync(collectionUri, query))
            {
                var documentUri = UriFactory.CreateDocumentUri(DatabaseId, Constants.Collection, commit.Id.ToString());

                await documentClient.DeleteDocumentAsync(documentUri, deleteOptions);
            }
        }

        public Task AppendAsync(Guid commitId, string streamName, ICollection<EventData> events)
        {
            return AppendAsync(commitId, streamName, EtagVersion.Any, events);
        }

        public async Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            Guard.NotEmpty(commitId, nameof(commitId));
            Guard.NotNullOrEmpty(streamName, nameof(streamName));
            Guard.NotNull(events, nameof(events));
            Guard.LessThan(events.Count, MaxCommitSize, "events.Count");

            ThrowIfDisposed();

            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                if (events.Count == 0)
                {
                    return;
                }

                var currentVersion = await GetEventStreamOffsetAsync(streamName);

                if (expectedVersion > EtagVersion.Any && expectedVersion != currentVersion)
                {
                    throw new WrongEventVersionException(currentVersion, expectedVersion);
                }

                var commit = BuildCommit(commitId, streamName, expectedVersion >= -1 ? expectedVersion : currentVersion, events);

                for (var attempt = 0; attempt < MaxWriteAttempts; attempt++)
                {
                    try
                    {
                        await documentClient.CreateDocumentAsync(collectionUri, commit);

                        return;
                    }
                    catch (DocumentClientException ex)
                    {
                        if (ex.StatusCode == HttpStatusCode.Conflict)
                        {
                            currentVersion = await GetEventStreamOffsetAsync(streamName);

                            if (expectedVersion > EtagVersion.Any)
                            {
                                throw new WrongEventVersionException(currentVersion, expectedVersion);
                            }

                            if (attempt < MaxWriteAttempts)
                            {
                                expectedVersion = currentVersion;
                            }
                            else
                            {
                                throw new TimeoutException("Could not acquire a free slot for the commit within the provided time.");
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private async Task<long> GetEventStreamOffsetAsync(string streamName)
        {
            var query =
                documentClient.CreateDocumentQuery<CosmosDbEventCommit>(collectionUri,
                    FilterBuilder.LastPosition(streamName));

            var document = await query.FirstOrDefaultAsync();

            if (document != null)
            {
                return document.EventStreamOffset + document.EventsCount;
            }

            return EtagVersion.Empty;
        }

        private static CosmosDbEventCommit BuildCommit(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            var commitEvents = new CosmosDbEvent[events.Count];

            var i = 0;

            foreach (var e in events)
            {
                var mongoEvent = CosmosDbEvent.FromEventData(e);

                commitEvents[i++] = mongoEvent;
            }

            var mongoCommit = new CosmosDbEventCommit
            {
                Id = commitId,
                Events = commitEvents,
                EventsCount = events.Count,
                EventStream = streamName,
                EventStreamOffset = expectedVersion,
                Timestamp = SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks()
            };

            return mongoCommit;
        }
    }
}