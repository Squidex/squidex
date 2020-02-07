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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IInitializable
    {
        private readonly IAppProvider appProvider;
        private readonly IJsonSerializer serializer;
        private readonly MongoContentCollectionAll collectionAll;
        private readonly MongoContentCollectionPublished collectionPublished;

        static MongoContentRepository()
        {
            StatusSerializer.Register();
        }

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider, ITextIndexer indexer, IJsonSerializer serializer)
        {
            Guard.NotNull(appProvider);
            Guard.NotNull(serializer);

            this.appProvider = appProvider;

            this.serializer = serializer;

            collectionAll = new MongoContentCollectionAll(database, appProvider, indexer, serializer);
            collectionPublished = new MongoContentCollectionPublished(database, appProvider, indexer, serializer);
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await collectionAll.InitializeAsync(ct);
            await collectionPublished.InitializeAsync(ct);
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, ClrQuery query, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schema, status, query);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schema, query);
            }
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, HashSet<Guid> ids, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schema, status, ids);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schema, ids);
            }
        }

        public Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, Status[]? status, HashSet<Guid> ids, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, status, ids);
            }
            else
            {
                return collectionPublished.QueryAsync(app, ids);
            }
        }

        public Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, Guid id, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.FindContentAsync(schema, status, id);
            }
            else
            {
                return collectionPublished.FindContentAsync(schema, id);
            }
        }

        public Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, HashSet<Guid> ids, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryIdsAsync(appId, ids);
            }
            else
            {
                return collectionPublished.QueryIdsAsync(appId, ids);
            }
        }

        public Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            return collectionAll.QueryScheduledWithoutDataAsync(now, callback);
        }

        public Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, Guid schemaId, FilterNode<ClrValue> filterNode)
        {
            return collectionAll.QueryIdsAsync(appId, schemaId, filterNode);
        }

        public IEnumerable<IMongoCollection<MongoContentEntity>> GetInternalCollections()
        {
            yield return collectionAll.GetInternalCollection();
            yield return collectionPublished.GetInternalCollection();
        }
    }
}
