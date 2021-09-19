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
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndex : MongoRepositoryBase<MongoTextIndexEntity>, ITextIndex, IDeleter
    {
        private const int Limit = 2000;
        private const int LimitHalf = 1000;
        private static readonly List<DomainId> EmptyResults = new List<DomainId>();

        public MongoTextIndex(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity> collection,
            CancellationToken ct)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoTextIndexEntity>(
                    Index.Ascending(x => x.DocId)),

                new CreateIndexModel<MongoTextIndexEntity>(
                    Index
                        .Text("t.t")
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.ServeAll)
                        .Ascending(x => x.ServePublished)
                        .Ascending(x => x.SchemaId)),

                new CreateIndexModel<MongoTextIndexEntity>(
                    Index
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.ServeAll)
                        .Ascending(x => x.ServePublished)
                        .Ascending(x => x.SchemaId)
                        .Ascending(x => x.GeoField)
                        .Geo2DSphere(x => x.GeoObject))
            }, ct);
        }

        protected override string CollectionName()
        {
            return "TextIndex";
        }

        async Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            await Collection.DeleteManyAsync(Filter.Eq(x => x.AppId, app.Id), ct);
        }

        public Task ExecuteAsync(IndexCommand[] commands,
            CancellationToken ct = default)
        {
            var writes = new List<WriteModel<MongoTextIndexEntity>>(commands.Length);

            foreach (var command in commands)
            {
                CommandFactory.CreateCommands(command, writes);
            }

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
        }

        public async Task<List<DomainId>?> SearchAsync(IAppEntity app, GeoQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            var findFilter =
                Filter.And(
                    Filter.Eq(x => x.AppId, app.Id),
                    Filter.Eq(x => x.SchemaId, query.SchemaId),
                    Filter_ByScope(scope),
                    Filter.GeoWithinCenterSphere(x => x.GeoObject, query.Longitude, query.Latitude, query.Radius / 6378100));

            var byGeo =
                await GetCollection(scope).Find(findFilter).Limit(Limit).Only(x => x.ContentId)
                    .ToListAsync(ct);

            var field = Field.Of<MongoTextIndexEntity>(x => nameof(x.ContentId));

            return byGeo.Select(x => DomainId.Create(x[field].AsString)).Distinct().ToList();
        }

        public async Task<List<DomainId>?> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            var (queryText, filter) = query;

            if (string.IsNullOrWhiteSpace(queryText))
            {
                return EmptyResults;
            }

            if (filter == null)
            {
                return await SearchByAppAsync(queryText, app, scope, Limit, ct);
            }
            else if (filter.Must)
            {
                return await SearchBySchemaAsync(queryText, app, filter, scope, Limit, ct);
            }
            else
            {
                var (bySchema, byApp) = await AsyncHelper.WhenAll(
                    SearchBySchemaAsync(queryText, app, filter, scope, LimitHalf, ct),
                    SearchByAppAsync(queryText, app, scope, LimitHalf, ct));

                return bySchema.Union(byApp).Distinct().ToList();
            }
        }

        private async Task<List<DomainId>> SearchBySchemaAsync(string queryText, IAppEntity app, TextFilter filter, SearchScope scope, int limit,
            CancellationToken ct = default)
        {
            var findFilter =
                Filter.And(
                    Filter.Eq(x => x.AppId, app.Id),
                    Filter.In(x => x.SchemaId, filter.SchemaIds),
                    Filter_ByScope(scope),
                    Filter.Text(queryText, "none"));

            var bySchema =
                await GetCollection(scope).Find(findFilter).Limit(limit).Only(x => x.ContentId)
                    .ToListAsync(ct);

            var field = Field.Of<MongoTextIndexEntity>(x => nameof(x.ContentId));

            return bySchema.Select(x => DomainId.Create(x[field].AsString)).Distinct().ToList();
        }

        private async Task<List<DomainId>> SearchByAppAsync(string queryText, IAppEntity app, SearchScope scope, int limit,
            CancellationToken ct = default)
        {
            var findFilter =
                Filter.And(
                    Filter.Eq(x => x.AppId, app.Id),
                    Filter.Exists(x => x.SchemaId),
                    Filter_ByScope(scope),
                    Filter.Text(queryText, "none"));

            var bySchema =
                await GetCollection(scope).Find(findFilter).Limit(limit).Only(x => x.ContentId)
                    .ToListAsync(ct);

            var field = Field.Of<MongoTextIndexEntity>(x => nameof(x.ContentId));

            return bySchema.Select(x => DomainId.Create(x[field].AsString)).Distinct().ToList();
        }

        private static FilterDefinition<MongoTextIndexEntity> Filter_ByScope(SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return Filter.Eq(x => x.ServeAll, true);
            }
            else
            {
                return Filter.Eq(x => x.ServePublished, true);
            }
        }

        private IMongoCollection<MongoTextIndexEntity> GetCollection(SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return Collection;
            }
            else
            {
                return Collection.WithReadPreference(ReadPreference.Secondary);
            }
        }
    }
}
