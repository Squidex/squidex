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
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IInitializable
    {
        private static readonly List<(Guid SchemaId, Guid Id)> EmptyIds = new List<(Guid SchemaId, Guid Id)>();
        private readonly IAppProvider appProvider;
        private readonly IJsonSerializer serializer;
        private readonly ITextIndexer indexer;
        private readonly string typeAssetDeleted;
        private readonly string typeContentDeleted;
        private readonly MongoContentCollection contents;

        static MongoContentRepository()
        {
            StatusSerializer.Register();
        }

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider, IJsonSerializer serializer, ITextIndexer indexer, TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(appProvider);
            Guard.NotNull(serializer);
            Guard.NotNull(indexer);
            Guard.NotNull(typeNameRegistry);

            this.appProvider = appProvider;
            this.indexer = indexer;
            this.serializer = serializer;

            typeAssetDeleted = typeNameRegistry.GetName<AssetDeleted>();
            typeContentDeleted = typeNameRegistry.GetName<ContentDeleted>();

            contents = new MongoContentCollection(database, serializer, appProvider);
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            return contents.InitializeAsync(ct);
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, bool inDraft, ClrQuery query, bool includeDraft = true)
        {
            Guard.NotNull(app);
            Guard.NotNull(schema);
            Guard.NotNull(query);

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByQuery"))
            {
                var fullTextIds = await indexer.SearchAsync(query.FullText, app, schema.Id, inDraft ? Scope.Draft : Scope.Published);

                if (fullTextIds?.Count == 0)
                {
                    return ResultList.CreateFrom<IContentEntity>(0);
                }

                return await contents.QueryAsync(schema, query, fullTextIds, status, inDraft, includeDraft);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, HashSet<Guid> ids, bool includeDraft = true)
        {
            Guard.NotNull(app);
            Guard.NotNull(ids);
            Guard.NotNull(schema);

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIds"))
            {
                return await contents.QueryAsync(schema, ids, status, includeDraft);
            }
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, Status[]? status, HashSet<Guid> ids, bool includeDraft = true)
        {
            Guard.NotNull(app);
            Guard.NotNull(ids);

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIdsWithoutSchema"))
            {
                return await contents.QueryAsync(app, ids, status, includeDraft);
            }
        }

        public async Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, Guid id, bool includeDraft = true)
        {
            Guard.NotNull(app);
            Guard.NotNull(schema);

            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await contents.FindContentAsync(schema, id, status, includeDraft);
            }
        }

        public async Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await contents.QueryScheduledWithoutDataAsync(now, callback);
            }
        }

        public async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, HashSet<Guid> ids)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await contents.QueryIdsAsync(appId, ids);
            }
        }

        public async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, Guid schemaId, FilterNode<ClrValue> filterNode)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                var schema = await appProvider.GetSchemaAsync(appId, schemaId);

                if (schema == null)
                {
                    return EmptyIds;
                }

                return await contents.QueryIdsAsync(schema, filterNode);
            }
        }

        public Task ClearAsync()
        {
            return contents.ClearAsync();
        }
    }
}
