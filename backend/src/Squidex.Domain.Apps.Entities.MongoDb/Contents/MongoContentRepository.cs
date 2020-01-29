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
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : MongoRepositoryBase<MongoContentEntity>, IContentRepository
    {
        private readonly IAppProvider appProvider;
        private readonly IJsonSerializer serializer;
        private readonly string typeAssetDeleted;
        private readonly string typeContentDeleted;
        private readonly CleanupReferences cleanupReferences;
        private readonly QueryContent queryContentAsync;
        private readonly QueryContentsByIds queryContentsById;
        private readonly QueryContentsByQuery queryContentsByQuery;
        private readonly QueryIdsAsync queryIdsAsync;
        private readonly QueryScheduledContents queryScheduledItems;

        static MongoContentRepository()
        {
            StatusSerializer.Register();
        }

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider, IJsonSerializer serializer, ITextIndexer indexer, TypeNameRegistry typeNameRegistry)
            : base(database)
        {
            Guard.NotNull(appProvider);
            Guard.NotNull(serializer);
            Guard.NotNull(typeNameRegistry);

            this.appProvider = appProvider;

            this.serializer = serializer;

            typeAssetDeleted = typeNameRegistry.GetName<AssetDeleted>();
            typeContentDeleted = typeNameRegistry.GetName<ContentDeleted>();

            cleanupReferences = new CleanupReferences(Collection);
            queryContentAsync = new QueryContent(Collection, serializer);
            queryContentsById = new QueryContentsByIds(Collection, serializer, appProvider);
            queryContentsByQuery = new QueryContentsByQuery(Collection, serializer, indexer);
            queryIdsAsync = new QueryIdsAsync(Collection, appProvider);
            queryScheduledItems = new QueryScheduledContents(Collection);
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection, CancellationToken ct = default)
        {
            await cleanupReferences.PrepareAsync(ct);
            await queryContentAsync.PrepareAsync(ct);
            await queryContentAsync.PrepareAsync(ct);
            await queryContentsByQuery.PrepareAsync(ct);
            await queryIdsAsync.PrepareAsync(ct);
            await queryScheduledItems.PrepareAsync();
        }

        protected override string CollectionName()
        {
            return "State_Contents";
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, bool inDraft, ClrQuery query, bool includeDraft = true)
        {
            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByQuery"))
            {
                return await queryContentsByQuery.DoAsync(app, schema, query, status, inDraft, includeDraft);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, HashSet<Guid> ids, bool includeDraft = true)
        {
            Guard.NotNull(app);

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIds"))
            {
                var result = await queryContentsById.DoAsync(app.Id, schema, ids, status, includeDraft);

                return ResultList.Create(result.Count, result.Select(x => x.Content));
            }
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, Status[]? status, HashSet<Guid> ids, bool includeDraft = true)
        {
            Guard.NotNull(app);

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIdsWithoutSchema"))
            {
                var result = await queryContentsById.DoAsync(app.Id, null, ids, status, includeDraft);

                return result;
            }
        }

        public async Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, Guid id, bool includeDraft = true)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryContentAsync.DoAsync(schema, id, status, includeDraft);
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
    }
}
