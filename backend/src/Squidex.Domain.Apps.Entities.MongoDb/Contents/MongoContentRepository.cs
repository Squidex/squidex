// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IInitializable
    {
        private readonly MongoContentCollection collectionAll;
        private readonly MongoContentCollection collectionPublished;
        private readonly IAppProvider appProvider;

        static MongoContentRepository()
        {
            TypeConverterStringSerializer<Status>.Register();
        }

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider)
        {
            collectionAll =
                new MongoContentCollection("States_Contents_All3", database,
                    ReadPreference.Primary);

            collectionPublished =
                new MongoContentCollection("States_Contents_Published3", database,
                    ReadPreference.Secondary);

            this.appProvider = appProvider;
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            await collectionAll.InitializeAsync(ct);
            await collectionPublished.InitializeAsync(ct);
        }

        public IAsyncEnumerable<IContentEntity> StreamAll(IAppEntity app, HashSet<DomainId>? schemaIds,
            CancellationToken ct = default)
        {
            return collectionAll.StreamAll(appId, schemaIds, ct);
        }

        public IAsyncEnumerable<IContentEntity> QueryScheduledWithoutDataAsync(Instant now,
            CancellationToken ct = default)
        {
            return collectionAll.QueryScheduledWithoutDataAsync(now, ct);
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q, SearchScope scope,
            CancellationToken ct = default)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schemas, q, ct);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schemas, q, ct);
            }
        }

        public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q, SearchScope scope,
            CancellationToken ct = default)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryAsync(app, schema, q, ct);
            }
            else
            {
                return collectionPublished.QueryAsync(app, schema, q, ct);
            }
        }

        public Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, DomainId id, SearchScope scope,
            CancellationToken ct = default)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.FindContentAsync(schema, id, ct);
            }
            else
            {
                return collectionPublished.FindContentAsync(schema, id, ct);
            }
        }

        public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(IAppEntity app, HashSet<DomainId> ids, SearchScope scope,
            CancellationToken ct = default)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.QueryIdsAsync(app, ids, ct);
            }
            else
            {
                return collectionPublished.QueryIdsAsync(app, ids, ct);
            }
        }

        public Task<bool> HasReferrersAsync(DomainId appId, DomainId contentId, SearchScope scope,
            CancellationToken ct = default)
        {
            if (scope == SearchScope.All)
            {
                return collectionAll.HasReferrersAsync(appId, contentId, ct);
            }
            else
            {
                return collectionPublished.HasReferrersAsync(appId, contentId, ct);
            }
        }

        public Task ResetScheduledAsync(DomainId documentId,
            CancellationToken ct = default)
        {
            return collectionAll.ResetScheduledAsync(documentId, ct);
        }

        public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(IAppEntity app, ISchemaEntity schema, FilterNode<ClrValue> filterNode,
            CancellationToken ct = default)
        {
            return collectionAll.QueryIdsAsync(app, schema, filterNode, ct);
        }

        public IEnumerable<IMongoCollection<MongoContentEntity>> GetInternalCollections()
        {
            yield return collectionAll.GetInternalCollection();
            yield return collectionPublished.GetInternalCollection();
        }
    }
}
