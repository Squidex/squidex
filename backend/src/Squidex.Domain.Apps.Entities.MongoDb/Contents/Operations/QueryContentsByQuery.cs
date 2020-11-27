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
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryContentsByQuery : OperationBase
    {
        private readonly DataConverter converter;
        private readonly ITextIndex indexer;
        private readonly IAppProvider appProvider;

        [BsonIgnoreExtraElements]
        internal sealed class IdOnly
        {
            [BsonId]
            [BsonElement("_id")]
            public DomainId Id { get; set; }

            public MongoContentEntity[] Joined { get; set; }
        }

        public QueryContentsByQuery(DataConverter converter, ITextIndex indexer, IAppProvider appProvider)
        {
            this.converter = converter;
            this.indexer = indexer;
            this.appProvider = appProvider;
        }

        protected override async Task PrepareAsync(CancellationToken ct = default)
        {
            var indexBySchemaWithRefs =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.ReferencedIds)
                    .Descending(x => x.LastModified));

            await Collection.Indexes.CreateOneAsync(indexBySchemaWithRefs, cancellationToken: ct);

            var indexBySchema =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.IsDeleted)
                    .Descending(x => x.LastModified));

            await Collection.Indexes.CreateOneAsync(indexBySchema, cancellationToken: ct);
        }

        public async IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds)
        {
            var find =
                schemaIds != null ?
                    Collection.Find(x => x.IndexedAppId == appId && schemaIds.Contains(x.IndexedSchemaId) && !x.IsDeleted) :
                    Collection.Find(x => x.IndexedAppId == appId && !x.IsDeleted);

            using (var cursor = await find.ToCursorAsync())
            {
                while (await cursor.MoveNextAsync())
                {
                    foreach (var entity in cursor.Current)
                    {
                        var schema = await appProvider.GetSchemaAsync(appId, entity.SchemaId.Id, false);

                        if (schema != null)
                        {
                            entity.ParseData(schema.SchemaDef, converter);

                            yield return entity;
                        }
                    }
                }
            }
        }

        public async Task<IResultList<IContentEntity>> DoAsync(IAppEntity app, ISchemaEntity schema, ClrQuery query, DomainId? referenced, SearchScope scope)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(query, nameof(query));

            try
            {
                query = query.AdjustToModel(schema.SchemaDef);

                List<DomainId>? fullTextIds = null;

                if (!string.IsNullOrWhiteSpace(query.FullText))
                {
                    var searchFilter = SearchFilter.ShouldHaveSchemas(schema.Id);

                    fullTextIds = await indexer.SearchAsync(query.FullText, app, searchFilter, scope);

                    if (fullTextIds?.Count == 0)
                    {
                        return ResultList.CreateFrom<IContentEntity>(0);
                    }
                }

                var filter = CreateFilter(schema.AppId.Id, schema.Id, fullTextIds, query, referenced);

                var contentCount = Collection.Find(filter).CountDocumentsAsync();
                var contentItems = FindContentsAsync(query, filter);

                var (items, total) = await AsyncHelper.WhenAll(contentItems, contentCount);

                foreach (var entity in items)
                {
                    entity.ParseData(schema.SchemaDef, converter);
                }

                return ResultList.Create<IContentEntity>(total, items);
            }
            catch (MongoCommandException ex) when (ex.Code == 96)
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
            catch (MongoQueryException ex) when (ex.Message.Contains("17406"))
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
        }

        private async Task<List<MongoContentEntity>> FindContentsAsync(ClrQuery query, FilterDefinition<MongoContentEntity> filter)
        {
            if (query.Skip > 0 && !IsSatisfiedByIndex(query))
            {
                var projection = Projection.Include("_id");

                foreach (var field in query.GetAllFields())
                {
                    projection = projection.Include(field);
                }

                var joined =
                    await Collection.Aggregate()
                        .Match(filter)
                        .Project<IdOnly>(projection)
                        .QuerySort(query)
                        .QuerySkip(query)
                        .QueryLimit(query)
                        .Lookup<IdOnly, MongoContentEntity, IdOnly>(Collection, x => x.Id, x => x.DocumentId, x => x.Joined)
                        .ToListAsync();

                return joined.Select(x => x.Joined[0]).ToList();
            }

            var result =
                Collection.Find(filter)
                    .QuerySort(query)
                    .QueryLimit(query)
                    .QuerySkip(query)
                    .ToListAsync();

            return await result;
        }

        private static bool IsSatisfiedByIndex(ClrQuery query)
        {
            return query.Sort?.All(x => x.Path.ToString() == "mt" && x.Order == SortOrder.Descending) == true;
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, DomainId schemaId, ICollection<DomainId>? ids, ClrQuery? query, DomainId? referenced)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.Eq(x => x.IndexedSchemaId, schemaId),
                Filter.Ne(x => x.IsDeleted, true)
            };

            if (ids != null && ids.Count > 0)
            {
                var documentIds = ids.Select(x => DomainId.Combine(appId, x)).ToList();

                filters.Add(
                    Filter.Or(
                        Filter.AnyIn(x => x.ReferencedIds, documentIds),
                        Filter.In(x => x.DocumentId, documentIds)));
            }

            if (query?.Filter != null)
            {
                filters.Add(query.Filter.BuildFilter<MongoContentEntity>());
            }

            if (referenced != null)
            {
                filters.Add(Filter.AnyEq(x => x.ReferencedIds, referenced.Value));
            }

            return Filter.And(filters);
        }
    }
}
