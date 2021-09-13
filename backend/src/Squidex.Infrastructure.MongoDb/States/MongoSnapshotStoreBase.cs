// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    public abstract class MongoSnapshotStoreBase<T, TState> : MongoRepositoryBase<TState>, ISnapshotStore<T> where TState : MongoState<T>, new()
    {
        protected MongoSnapshotStoreBase(IMongoDatabase database, JsonSerializer jsonSerializer)
            : base(database, Register(jsonSerializer))
        {
        }

        private static bool Register(JsonSerializer jsonSerializer)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));

            BsonJsonConvention.Register(jsonSerializer);

            return true;
        }

        protected override string CollectionName()
        {
            var attribute = typeof(T).GetCustomAttributes(true).OfType<CollectionNameAttribute>().FirstOrDefault();

            var name = attribute?.Name ?? typeof(T).Name;

            return $"States_{name}";
        }

        public async Task<(T Value, bool Valid, long Version)> ReadAsync(DomainId key,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/ReadAsync"))
            {
                var existing =
                    await Collection.Find(x => x.DocumentId.Equals(key))
                        .FirstOrDefaultAsync(ct);

                if (existing != null)
                {
                    return (existing.Document, true, existing.Version);
                }

                return (default!, true, EtagVersion.Empty);
            }
        }

        public async Task WriteAsync(DomainId key, T value, long oldVersion, long newVersion,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/WriteAsync"))
            {
                var document = CreateDocument(key, value, newVersion);

                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, document, ct);
            }
        }

        public async Task WriteManyAsync(IEnumerable<(DomainId Key, T Value, long Version)> snapshots,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/WriteManyAsync"))
            {
                var writes = snapshots.Select(x =>
                    new ReplaceOneModel<TState>(Filter.Eq(y => y.DocumentId, x.Key), CreateDocument(x.Key, x.Value, x.Version))
                    {
                        IsUpsert = true
                    }).ToList();

                if (writes.Count == 0)
                {
                    return;
                }

                await Collection.BulkWriteAsync(writes, BulkUnordered, ct);
            }
        }

        public async Task RemoveAsync(DomainId key,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/RemoveAsync"))
            {
                await Collection.DeleteOneAsync(x => x.DocumentId.Equals(key), ct);
            }
        }

        public async IAsyncEnumerable<(T State, long Version)> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("ContentQueryService/ReadAllAsync"))
            {
                var find = Collection.Find(new BsonDocument(), Batching.Options);

                await foreach (var document in find.ToAsyncEnumerable(ct))
                {
                    yield return (document.Document, document.Version);
                }
            }
        }

        private static TState CreateDocument(DomainId id, T doc, long version)
        {
            var result = new TState
            {
                Document = doc,
                DocumentId = id,
                Version = version
            };

            result.Prepare();

            return result;
        }
    }
}
