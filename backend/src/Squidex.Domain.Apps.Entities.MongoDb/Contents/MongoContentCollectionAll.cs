// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public sealed class MongoContentCollectionAll : MongoRepositoryBase<MongoContentEntity>
    {
        private readonly QueryContent queryContentAsync;
        private readonly QueryContentsByIds queryContentsById;
        private readonly QueryContentsByQuery queryContentsByQuery;
        private readonly QueryIdsAsync queryIdsAsync;
        private readonly QueryScheduledContents queryScheduledItems;

        public MongoContentCollectionAll(IMongoDatabase database, IAppProvider appProvider, ITextIndexer indexer, IJsonSerializer serializer)
            : base(database)
        {
            queryContentAsync = new QueryContent(serializer);
            queryContentsById = new QueryContentsByIds(serializer, appProvider);
            queryContentsByQuery = new QueryContentsByQuery(serializer, indexer);
            queryIdsAsync = new QueryIdsAsync(appProvider);
            queryScheduledItems = new QueryScheduledContents();
        }

        public IMongoCollection<MongoContentEntity> GetInternalCollection()
        {
            return Collection;
        }

        protected override string CollectionName()
        {
            return "State_Contents_All";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection, CancellationToken ct = default)
        {
            await queryContentAsync.PrepareAsync(collection, ct);
            await queryContentsById.PrepareAsync(collection, ct);
            await queryContentsByQuery.PrepareAsync(collection, ct);
            await queryIdsAsync.PrepareAsync(collection, ct);
            await queryScheduledItems.PrepareAsync(collection, ct);
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, ClrQuery query)
        {
            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByQuery"))
            {
                return await queryContentsByQuery.DoAsync(app, schema, query, status, true);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, HashSet<Guid> ids)
        {
            Guard.NotNull(app);

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIds"))
            {
                var result = await queryContentsById.DoAsync(app.Id, schema, ids, status);

                return ResultList.Create(result.Count, result.Select(x => x.Content));
            }
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, Status[]? status, HashSet<Guid> ids)
        {
            Guard.NotNull(app);

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIdsWithoutSchema"))
            {
                var result = await queryContentsById.DoAsync(app.Id, null, ids, status);

                return result;
            }
        }

        public async Task<IContentEntity?> FindContentAsync(ISchemaEntity schema, Status[]? status, Guid id)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryContentAsync.DoAsync(schema, id, status);
            }
        }

        public async Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await queryScheduledItems.DoAsync(now, callback);
            }
        }

        public async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, HashSet<Guid> ids)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryIdsAsync.DoAsync(appId, ids);
            }
        }

        public async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, Guid schemaId, FilterNode<ClrValue> filterNode)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryIdsAsync.DoAsync(appId, schemaId, filterNode);
            }
        }

        public Task<MongoContentEntity> FindAsync(Guid id)
        {
            return Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public Task UpsertVersionedAsync(Guid id, long oldVersion, MongoContentEntity entity)
        {
            return Collection.UpsertVersionedAsync(id, oldVersion, entity);
        }

        public Task RemoveAsync(Guid id)
        {
            return Collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
