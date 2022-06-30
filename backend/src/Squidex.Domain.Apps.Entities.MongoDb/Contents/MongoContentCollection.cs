// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public sealed class MongoContentCollection : MongoRepositoryBase<MongoContentEntity>
    {
        private readonly QueryAsStream queryAsStream;
        private readonly QueryById queryBdId;
        private readonly QueryByIds queryByIds;
        private readonly QueryByQuery queryByQuery;
        private readonly QueryReferences queryReferences;
        private readonly QueryReferrers queryReferrers;
        private readonly QueryScheduled queryScheduled;
        private readonly ReadPreference readPreference;
        private readonly string name;

        public MongoContentCollection(string name, IMongoDatabase database, ReadPreference readPreference)
            : base(database)
        {
            this.name = name;

            queryAsStream = new QueryAsStream();
            queryBdId = new QueryById();
            queryByIds = new QueryByIds();
            queryByQuery = new QueryByQuery(new MongoCountCollection(database, name));
            queryReferences = new QueryReferences(queryByIds);
            queryReferrers = new QueryReferrers();
            queryScheduled = new QueryScheduled();

            this.readPreference = readPreference;
        }

        public IMongoCollection<MongoContentEntity> GetInternalCollection()
        {
            return Collection;
        }

        protected override string CollectionName()
        {
            return name;
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings
            {
                ReadPreference = readPreference
            };
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection,
            CancellationToken ct)
        {
            var operations = new OperationCollectionBase[]
            {
                queryAsStream,
                queryBdId,
                queryByIds,
                queryByQuery,
                queryReferences,
                queryReferrers,
                queryScheduled
            };

            foreach (var operation in operations)
            {
                operation.Setup(collection);
            }

            return collection.Indexes.CreateManyAsync(operations.SelectMany(x => x.CreateIndexes()), ct);
        }

        public Task ResetScheduledAsync(DomainId documentId,
            CancellationToken ct)
        {
            return Collection.UpdateOneAsync(x => x.DocumentId == documentId, Update.Unset(x => x.ScheduleJob).Unset(x => x.ScheduledAt), cancellationToken: ct);
        }

        public IAsyncEnumerable<IContentEntity> StreamAll(IAppEntity app, HashSet<DomainId>? schemaIds,
            CancellationToken ct)
        {
            return queryAsStream.StreamAll(appId, schemaIds, ct);
        }

        public IAsyncEnumerable<IContentEntity> QueryScheduledWithoutDataAsync(Instant now,
            CancellationToken ct)
        {
            return queryScheduled.QueryAsync(now, ct);
        }

        public async Task DeleteAppAsync(DomainId appId,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/DeleteAppAsync"))
            {
                await Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, appId), ct);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryAsync"))
            {
                try
                {
                    if (q.Ids != null && q.Ids.Count > 0 && schemas.Count > 0)
                    {
                        return await queryByIds.QueryAsync(app, schemas, q, ct);
                    }

                    if (q.ScheduledFrom != null && q.ScheduledTo != null && schemas.Count > 0)
                    {
                        return await queryScheduled.QueryAsync(app, schemas, q, ct);
                    }

                    if (q.Referencing != default && schemas.Count > 0)
                    {
                        return await queryReferences.QueryAsync(app, schemas, q, ct);
                    }

                    if (q.Reference != default && schemas.Count > 0)
                    {
                        return await queryByQuery.QueryAsync(app, schemas, q, ct);
                    }

                    return ResultList.Empty<IContentEntity>();
                }
                catch (MongoCommandException ex) when (ex.Code == 96)
                {
                    throw new DomainException(T.Get("common.resultTooLarge"));
                }
                catch (MongoQueryException ex) when (ex.Message.Contains("17406", StringComparison.Ordinal))
                {
                    throw new DomainException(T.Get("common.resultTooLarge"));
                }
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryAsync"))
            {
                try
                {
                    if (q.Ids != null && q.Ids.Count > 0)
                    {
                        return await queryByIds.QueryAsync(app, new List<ISchemaEntity> { schema }, q, ct);
                    }

                    if (q.ScheduledFrom != null && q.ScheduledTo != null)
                    {
                        return await queryScheduled.QueryAsync(app, new List<ISchemaEntity> { schema }, q, ct);
                    }

                    if (q.Referencing == default)
                    {
                        return await queryByQuery.QueryAsync(schema, q, ct);
                    }

                    return ResultList.Empty<IContentEntity>();
                }
                catch (MongoCommandException ex) when (ex.Code == 96)
                {
                    throw new DomainException(T.Get("common.resultTooLarge"));
                }
                catch (MongoQueryException ex) when (ex.Message.Contains("17406", StringComparison.Ordinal))
                {
                    throw new DomainException(T.Get("common.resultTooLarge"));
                }
            }
        }

        public async Task<IContentEntity?> FindContentAsync(ISchemaEntity schema, DomainId id,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/FindContentAsync"))
            {
                return await queryBdId.QueryAsync(schema, id, ct);
            }
        }

        public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(IAppEntity app, HashSet<DomainId> ids,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryIdsAsync"))
            {
                return await queryByIds.QueryIdsAsync(app, ids, ct);
            }
        }

        public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(IAppEntity app, ISchemaEntity schema, FilterNode<ClrValue> filterNode,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryIdsAsync"))
            {
                return await queryByQuery.QueryIdsAsync(app, schema, filterNode, ct);
            }
        }

        public async Task<bool> HasReferrersAsync(DomainId appId, DomainId contentId,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/HasReferrersAsync"))
            {
                return await queryReferrers.CheckExistsAsync(appId, contentId, ct);
            }
        }

        public Task<MongoContentEntity> FindAsync(DomainId documentId,
            CancellationToken ct = default)
        {
            return Collection.Find(x => x.DocumentId == documentId).FirstOrDefaultAsync(ct);
        }

        public IAsyncEnumerable<MongoContentEntity> StreamAll(
            CancellationToken ct)
        {
            return Collection.Find(new BsonDocument()).ToAsyncEnumerable(ct);
        }

        public Task UpsertVersionedAsync(DomainId documentId, long oldVersion, MongoContentEntity value,
            CancellationToken ct = default)
        {
            return Collection.UpsertVersionedAsync(documentId, oldVersion, value.Version, value, ct);
        }

        public Task RemoveAsync(DomainId key,
            CancellationToken ct = default)
        {
            return Collection.DeleteOneAsync(x => x.DocumentId == key, null, ct);
        }

        public Task InsertManyAsync(IReadOnlyList<MongoContentEntity> snapshots,
            CancellationToken ct = default)
        {
            return Collection.InsertManyAsync(snapshots, InsertUnordered, ct);
        }
    }
}
