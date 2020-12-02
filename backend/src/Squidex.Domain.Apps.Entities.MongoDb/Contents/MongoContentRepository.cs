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
        private readonly MongoContentCollection collectionAll;
        private readonly MongoContentCollection collectionPublished;

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

            collectionAll =
                new MongoContentCollection(
                    "States_Contents_All2", database, appProvider, indexer, converter);

            collectionPublished =
                new MongoContentCollection(
                    "States_Contents_Published2", database, appProvider, indexer, converter);
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await collectionAll.InitializeAsync(ct);
            await collectionPublished.InitializeAsync(ct);
        }

        public IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds)
        {
            return collectionAll.StreamAll(appId, schemaIds);
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schemas, q);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schemas, q);
            }
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schema, q, scope);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schema, q, scope);
            }
        }

        public Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, DomainId id, SearchScope scope)
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

        public Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids, SearchScope scope)
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

        public Task<bool> HasReferrersAsync(DomainId appId, DomainId contentId, SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.HasReferrersAsync(appId, contentId);
            }
            else
            {
                return collectionPublished.HasReferrersAsync(appId, contentId);
            }
        }

        public Task ResetScheduledAsync(DomainId documentId)
        {
            return collectionAll.ResetScheduledAsync(documentId);
        }

        public Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            return collectionAll.QueryScheduledWithoutDataAsync(now, callback);
        }

        public Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode)
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
