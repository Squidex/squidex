// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly string name;
        private readonly ReadPreference readPreference;

        public MongoContentCollection(string name, IMongoDatabase database, IAppProvider appProvider, ReadPreference readPreference)
            : base(database)
        {
            this.name = name;

            queryAsStream = new QueryAsStream();
            queryBdId = new QueryById();
            queryByIds = new QueryByIds();
            queryByQuery = new QueryByQuery(appProvider);
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
            var operations = new OperationBase[]
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

        public IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds,
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
                if (q.Ids != null && q.Ids.Count > 0)
                {
                    return await queryByIds.QueryAsync(app.Id, schemas, q, ct);
                }

                if (q.ScheduledFrom != null && q.ScheduledTo != null)
                {
                    return await queryScheduled.QueryAsync(app.Id, schemas, q, ct);
                }

                if (q.Referencing != default)
                {
                    return await queryReferences.QueryAsync(app.Id, schemas, q, ct);
                }

                if (q.Reference != default)
                {
                    return await queryByQuery.QueryAsync(app, schemas, q, ct);
                }

                return ResultList.CreateFrom<IContentEntity>(0);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryAsync"))
            {
                if (q.Ids != null && q.Ids.Count > 0)
                {
                    return await queryByIds.QueryAsync(app.Id, new List<ISchemaEntity> { schema }, q, ct);
                }

                if (q.ScheduledFrom != null && q.ScheduledTo != null)
                {
                    return await queryScheduled.QueryAsync(app.Id, new List<ISchemaEntity> { schema }, q, ct);
                }

                if (q.Referencing == default)
                {
                    return await queryByQuery.QueryAsync(app, schema, q, ct);
                }

                return ResultList.CreateFrom<IContentEntity>(0);
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

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryIdsAsync"))
            {
                return await queryByIds.QueryIdsAsync(appId, ids, ct);
            }
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryIdsAsync"))
            {
                return await queryByQuery.QueryIdsAsync(appId, schemaId, filterNode, ct);
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

        public async Task<long> FindVersionAsync(DomainId documentId,
            CancellationToken ct = default)
        {
            var result = await Collection.Find(x => x.DocumentId == documentId).Only(x => x.Version).FirstOrDefaultAsync(ct);

            return result?["vs"].AsInt64 ?? EtagVersion.Empty;
        }

        public Task UpsertVersionedAsync(DomainId documentId, long oldVersion, MongoContentEntity entity,
            CancellationToken ct = default)
        {
            return Collection.UpsertVersionedAsync(documentId, oldVersion, entity.Version, entity, ct);
        }

        public Task RemoveAsync(DomainId documentId,
            CancellationToken ct = default)
        {
            return Collection.DeleteOneAsync(x => x.DocumentId == documentId, ct);
        }

        public Task InsertManyAsync(IReadOnlyList<MongoContentEntity> entities,
            CancellationToken ct = default)
        {
            if (entities.Count == 0)
            {
                return Task.CompletedTask;
            }

            var writes = entities.Select(x => new ReplaceOneModel<MongoContentEntity>(
                Filter.Eq(y => y.DocumentId, x.DocumentId),
                x)
            {
                IsUpsert = true
            }).ToList();

            return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }
    }
}
