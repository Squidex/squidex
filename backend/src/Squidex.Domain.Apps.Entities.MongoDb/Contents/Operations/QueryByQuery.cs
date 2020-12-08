// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
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
    internal sealed class QueryByQuery : OperationBase
    {
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

        public QueryByQuery(DataConverter dataConverter, ITextIndex indexer, IAppProvider appProvider)
            : base(dataConverter)
        {
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

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode)
        {
            Guard.NotNull(filterNode, nameof(filterNode));

            try
            {
                var schema = await appProvider.GetSchemaAsync(appId, schemaId, false);

                if (schema == null)
                {
                    return new List<(DomainId SchemaId, DomainId Id, Status Status)>();
                }

                var filter = BuildFilter(appId, schemaId, filterNode.AdjustToModel(schema.SchemaDef));

                var contentItems = await Collection.FindStatusAsync(filter);

                return contentItems.Select(x => (x.IndexedSchemaId, x.Id, x.Status)).ToList();
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

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(q, nameof(q));

            try
            {
                var query = q.Query.AdjustToModel(null);

                List<DomainId>? fullTextIds = null;

                if (!string.IsNullOrWhiteSpace(query.FullText))
                {
                    throw new NotSupportedException();
                }

                var filter = CreateFilter(app.Id, schemas.Select(x => x.Id), fullTextIds, query, q.Reference);

                var contentEntities = await FindContentsAsync(query, filter);
                var contentTotal = (long)contentEntities.Count;

                if (contentEntities.Count > 0)
                {
                    if (contentTotal >= q.Query.Take || q.Query.Skip > 0)
                    {
                        contentTotal = await Collection.Find(filter).CountDocumentsAsync();
                    }

                    var contentSchemas = schemas.ToDictionary(x => x.Id);

                    foreach (var entity in contentEntities)
                    {
                        entity.ParseData(contentSchemas[entity.IndexedSchemaId].SchemaDef, DataConverter);
                    }
                }

                return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
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

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q, SearchScope scope)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(q, nameof(q));

            try
            {
                var query = q.Query.AdjustToModel(schema.SchemaDef);

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

                var filter = CreateFilter(schema.AppId.Id, Enumerable.Repeat(schema.Id, 1), fullTextIds, query, q.Reference);

                var contentEntities = await FindContentsAsync(query, filter);
                var contentTotal = (long)contentEntities.Count;

                if (contentEntities.Count > 0)
                {
                    if (contentTotal >= q.Query.Take || q.Query.Skip > 0)
                    {
                        contentTotal = await Collection.Find(filter).CountDocumentsAsync();
                    }

                    foreach (var entity in contentEntities)
                    {
                        entity.ParseData(schema.SchemaDef, DataConverter);
                    }
                }

                return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
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

        private static FilterDefinition<MongoContentEntity> BuildFilter(DomainId appId, DomainId schemaId, FilterNode<ClrValue>? filterNode)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.Eq(x => x.IndexedSchemaId, schemaId),
                Filter.Ne(x => x.IsDeleted, true)
            };

            if (filterNode != null)
            {
                filters.Add(filterNode.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, IEnumerable<DomainId> schemaIds, ICollection<DomainId>? ids, ClrQuery? query, DomainId referenced)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.In(x => x.IndexedSchemaId, schemaIds),
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

            if (referenced != default)
            {
                filters.Add(Filter.AnyEq(x => x.ReferencedIds, referenced));
            }

            return Filter.And(filters);
        }
    }
}
