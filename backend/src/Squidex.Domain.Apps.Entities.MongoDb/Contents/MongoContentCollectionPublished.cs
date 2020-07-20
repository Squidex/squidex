﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public sealed class MongoContentCollectionPublished : MongoRepositoryBase<MongoContentEntity>
    {
        private readonly QueryContent queryContentAsync;
        private readonly QueryContentsByIds queryContentsById;
        private readonly QueryContentsByQuery queryContentsByQuery;
        private readonly QueryIdsAsync queryIdsAsync;

        public MongoContentCollectionPublished(IMongoDatabase database, IAppProvider appProvider, ITextIndex indexer, DataConverter converter)
            : base(database)
        {
            queryContentAsync = new QueryContent(converter);
            queryContentsById = new QueryContentsByIds(converter, appProvider);
            queryContentsByQuery = new QueryContentsByQuery(converter, indexer);
            queryIdsAsync = new QueryIdsAsync(appProvider);
        }

        public IMongoCollection<MongoContentEntity> GetInternalCollection()
        {
            return Collection;
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings
            {
                ReadPreference = ReadPreference.SecondaryPreferred.With(TimeSpan.FromMinutes(2))
            };
        }

        protected override string CollectionName()
        {
            return "State_Contents_Published";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection, CancellationToken ct = default)
        {
            await queryContentAsync.PrepareAsync(collection, ct);
            await queryContentsById.PrepareAsync(collection, ct);
            await queryContentsByQuery.PrepareAsync(collection, ct);
            await queryIdsAsync.PrepareAsync(collection, ct);
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, ClrQuery query)
        {
            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByQuery"))
            {
                return await queryContentsByQuery.DoAsync(app, schema, query, SearchScope.Published);
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, HashSet<DomainId> ids)
        {
            Guard.NotNull(app, nameof(app));

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIds"))
            {
                var result = await queryContentsById.DoAsync(app.Id, schema, ids, true);

                return ResultList.Create(result.Count, result.Select(x => x.Content));
            }
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, HashSet<DomainId> ids)
        {
            Guard.NotNull(app, nameof(app));

            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIdsWithoutSchema"))
            {
                var result = await queryContentsById.DoAsync(app.Id, null, ids, true);

                return result;
            }
        }

        public async Task<IContentEntity?> FindContentAsync(ISchemaEntity schema, DomainId id)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryContentAsync.DoAsync(schema, id);
            }
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id)>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await queryIdsAsync.DoAsync(appId, ids);
            }
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
