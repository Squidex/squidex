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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndex : MongoRepositoryBase<MongoTextIndexEntity>, ITextIndex, IDeleter
    {
        private readonly ProjectionDefinition<MongoTextIndexEntity> searchTextProjection;
        private readonly ProjectionDefinition<MongoTextIndexEntity> searchGeoProjection;

        private sealed class MongoTextResult
        {
            [BsonId]
            [BsonElement]
            [BsonRepresentation(BsonType.String)]
            public string Id { get; set; }

            [BsonRequired]
            [BsonElement("_ci")]
            [BsonRepresentation(BsonType.String)]
            public DomainId ContentId { get; set; }

            [BsonIgnoreIfDefault]
            [BsonElement("score")]
            public double Score { get; set; }
        }

        public MongoTextIndex(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
            searchGeoProjection = Projection.Include(x => x.ContentId);
            searchTextProjection = Projection.Include(x => x.ContentId).MetaTextScore("score");
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
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            var findFilter =
                Filter.And(
                    Filter.Eq(x => x.AppId, app.Id),
                    Filter.Eq(x => x.SchemaId, query.SchemaId),
                    Filter_ByScope(scope),
                    Filter.GeoWithinCenterSphere(x => x.GeoObject, query.Longitude, query.Latitude, query.Radius / 6378100));

            var byGeo =
                await GetCollection(scope).Find(findFilter).Limit(query.Take).Project<MongoTextResult>(searchGeoProjection)
                    .ToListAsync(ct);

            return byGeo.Select(x => x.ContentId).ToList();
        }

        public async Task<List<DomainId>?> SearchAsync(IAppEntity app, TextQuery query, SearchScope scope,
            CancellationToken ct = default)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(query, nameof(query));

            var (search, take) = query;

            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var result = new List<(DomainId Id, double Score)>();

            if (query.RequiredSchemaIds?.Count > 0)
            {
                await SearchBySchemaAsync(result, search, app, query.RequiredSchemaIds, scope, take, 1, ct);
            }
            else if (query.PreferredSchemaId == null)
            {
                await SearchByAppAsync(result, search, app, scope, take, 1, ct);
            }
            else
            {
                var halfBucket = take / 2;

                var schemaIds = Enumerable.Repeat(query.PreferredSchemaId.Value, 1);

                await SearchBySchemaAsync(result, search, app, schemaIds, scope, halfBucket, 1.1, ct);
                await SearchByAppAsync(result, search, app, scope, halfBucket, 1, ct);
            }

            return result.OrderByDescending(x => x.Score).Select(x => x.Id).Distinct().ToList();
        }

        private Task SearchBySchemaAsync(List<(DomainId, double)> result, string text, IAppEntity app, IEnumerable<DomainId> schemaIds, SearchScope scope, int take, double factor,
            CancellationToken ct = default)
        {
            var filter =
                Filter.And(
                    Filter.Eq(x => x.AppId, app.Id),
                    Filter.In(x => x.SchemaId, schemaIds),
                    Filter_ByScope(scope),
                    Filter.Text(text, "none"));

            return SearchAsync(result, filter, scope, take, factor, ct);
        }

        private Task SearchByAppAsync(List<(DomainId, double)> result, string text, IAppEntity app, SearchScope scope, int take, double factor,
            CancellationToken ct = default)
        {
            var filter =
                Filter.And(
                    Filter.Eq(x => x.AppId, app.Id),
                    Filter.Exists(x => x.SchemaId),
                    Filter_ByScope(scope),
                    Filter.Text(text, "none"));

            return SearchAsync(result, filter, scope, take, factor, ct);
        }

        private async Task SearchAsync(List<(DomainId, double)> result, FilterDefinition<MongoTextIndexEntity> filter, SearchScope scope, int take, double factor,
            CancellationToken ct = default)
        {
            var collection = GetCollection(scope);

            var find =
                collection.Find(filter).Limit(take)
                    .Project<MongoTextResult>(searchTextProjection).Sort(Sort.MetaTextScore("score"));

            var documents = await find.ToListAsync(ct);

            result.AddRange(documents.Select(x => (x.ContentId, x.Score * factor)));
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
