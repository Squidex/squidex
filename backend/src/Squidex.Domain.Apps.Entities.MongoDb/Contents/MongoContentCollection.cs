// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
using Squidex.Log;

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

        public MongoContentCollection(string name, IMongoDatabase database, IAppProvider appProvider, DataConverter dataConverter)
            : base(database)
        {
            this.name = name;

            queryAsStream = new QueryAsStream(dataConverter, appProvider);
            queryBdId = new QueryById(dataConverter);
            queryByIds = new QueryByIds(dataConverter);
            queryByQuery = new QueryByQuery(dataConverter, appProvider);
            queryReferences = new QueryReferences(dataConverter, queryByIds);
            queryReferrers = new QueryReferrers(dataConverter);
            queryScheduled = new QueryScheduled(dataConverter);
        }

        public IMongoCollection<MongoContentEntity> GetInternalCollection()
        {
            return Collection;
        }

        protected override string CollectionName()
        {
            return name;
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection, CancellationToken ct = default)
        {
            await queryAsStream.PrepareAsync(collection, ct);
            await queryBdId.PrepareAsync(collection, ct);
            await queryByIds.PrepareAsync(collection, ct);
            await queryByQuery.PrepareAsync(collection, ct);
            await queryReferences.PrepareAsync(collection, ct);
            await queryReferrers.PrepareAsync(collection, ct);
            await queryScheduled.PrepareAsync(collection, ct);
        }

        public IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds)
        {
            return queryAsStream.StreamAll(appId, schemaIds);
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (q.Ids != null && q.Ids.Count > 0)
                {
                    return await queryByIds.QueryAsync(app.Id, schemas, q);
                }

                if (q.Referencing != default)
                {
                    return await queryReferences.QueryAsync(app.Id, schemas, q);
                }

                if (q.Reference != default)
                {
                    return await queryByQuery.QueryAsync(app, schemas, q);
                }

                return ResultList.CreateFrom<IContentEntity>(0);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (q.Ids != null && q.Ids.Count > 0)
                {
                    return await queryByIds.QueryAsync(app.Id, new List<ISchemaEntity> { schema }, q);
                }

                if (q.Referencing == default)
                {
                    return await queryByQuery.QueryAsync(app, schema, q);
                }

                return ResultList.CreateFrom<IContentEntity>(0);
            }
        }

        public async Task<IContentEntity?> FindContentAsync(ISchemaEntity schema, DomainId id)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryBdId.QueryAsync(schema, id);
            }
        }

        public async Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await queryScheduled.QueryAsync(now, callback);
            }
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryByIds.QueryIdsAsync(appId, ids);
            }
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryByQuery.QueryIdsAsync(appId, schemaId, filterNode);
            }
        }

        public async Task<bool> HasReferrersAsync(DomainId appId, DomainId contentId)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryReferrers.CheckExistsAsync(appId, contentId);
            }
        }

        public Task<MongoContentEntity> FindAsync(DomainId documentId)
        {
            return Collection.Find(x => x.DocumentId == documentId).FirstOrDefaultAsync();
        }

        public Task ResetScheduledAsync(DomainId documentId)
        {
            return Collection.UpdateOneAsync(x => x.DocumentId == documentId, Update.Unset(x => x.ScheduleJob).Unset(x => x.ScheduledAt));
        }

        public Task UpsertVersionedAsync(DomainId documentId, long oldVersion, MongoContentEntity entity)
        {
            return Collection.UpsertVersionedAsync(documentId, oldVersion, entity.Version, entity);
        }

        public Task RemoveAsync(DomainId documentId)
        {
            return Collection.DeleteOneAsync(x => x.DocumentId == documentId);
        }
    }
}
