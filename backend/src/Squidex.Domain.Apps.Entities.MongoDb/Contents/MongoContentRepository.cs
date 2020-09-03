﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IInitializable
    {
        private readonly IAppProvider appProvider;
        private readonly DataConverter converter;
        private readonly MongoContentCollectionAll collectionAll;
        private readonly MongoContentCollectionPublished collectionPublished;

        static MongoContentRepository()
        {
            StatusSerializer.Register();
        }

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider, ITextIndex indexer, IJsonSerializer serializer)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(serializer, nameof(serializer));

            this.appProvider = appProvider;

            converter = new DataConverter(serializer);

            collectionAll = new MongoContentCollectionAll(database, appProvider, indexer, converter);
            collectionPublished = new MongoContentCollectionPublished(database, appProvider, indexer, converter);
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await collectionAll.InitializeAsync(ct);
            await collectionPublished.InitializeAsync(ct);
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, ClrQuery query, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schema, query);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schema, query);
            }
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, HashSet<Guid> ids, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schema, ids);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schema, ids);
            }
        }

        public Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, HashSet<Guid> ids, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, ids);
            }
            else
            {
                return collectionPublished.QueryAsync(app, ids);
            }
        }

        public Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.FindContentAsync(schema, id);
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

        public Task ResetScheduledAsync(Guid id)
        {
            return collectionAll.ResetScheduledAsync(id);
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
