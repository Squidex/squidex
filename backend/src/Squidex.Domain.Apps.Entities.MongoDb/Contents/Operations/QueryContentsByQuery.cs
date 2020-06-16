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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryContentsByQuery : OperationBase
    {
        private readonly DataConverter converter;
        private readonly ITextIndex indexer;

        [BsonIgnoreExtraElements]
        internal sealed class IdOnly
        {
            [BsonId]
            [BsonElement("_id")]
            [BsonRepresentation(BsonType.String)]
            public Guid Id { get; set; }

            public MongoContentEntity[] Joined { get; set; }
        }

        public QueryContentsByQuery(DataConverter converter, ITextIndex indexer)
        {
            this.converter = converter;

            this.indexer = indexer;
        }

        protected override Task PrepareAsync(CancellationToken ct = default)
        {
            var index =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.ReferencedIds)
                    .Descending(x => x.LastModified));

            return Collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
        }

        public async Task<IResultList<IContentEntity>> DoAsync(IAppEntity app, ISchemaEntity schema, ClrQuery query, SearchScope scope)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(query, nameof(query));

            try
            {
                query = query.AdjustToModel(schema.SchemaDef);

                List<Guid>? fullTextIds = null;

                if (!string.IsNullOrWhiteSpace(query.FullText))
                {
                    var searchFilter = SearchFilter.ShouldHaveSchemas(schema.Id);

                    fullTextIds = await indexer.SearchAsync(query.FullText, app, searchFilter, scope);

                    if (fullTextIds?.Count == 0)
                    {
                        return ResultList.CreateFrom<IContentEntity>(0);
                    }
                }

                var filter = CreateFilter(schema.Id, fullTextIds, query);

                var contentCount = Collection.Find(filter).CountDocumentsAsync();
                var contentItems = FindContentsAsync(query, filter);

                await Task.WhenAll(contentItems, contentCount);

                foreach (var entity in contentItems.Result)
                {
                    entity.ParseData(schema.SchemaDef, converter);
                }

                return ResultList.Create<IContentEntity>(contentCount.Result, contentItems.Result);
            }
            catch (MongoCommandException ex) when (ex.Code == 96)
            {
                throw new DomainException("Result set is too large to be retrieved. Use $take parameter to reduce the number of items.");
            }
            catch (MongoQueryException ex) when (ex.Message.Contains("17406"))
            {
                throw new DomainException("Result set is too large to be retrieved. Use $take parameter to reduce the number of items.");
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
                        .Lookup<IdOnly, MongoContentEntity, IdOnly>(Collection, x => x.Id, x => x.Id, x => x.Joined)
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

        private static FilterDefinition<MongoContentEntity> CreateFilter(Guid schemaId, ICollection<Guid>? ids, ClrQuery? query)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedSchemaId, schemaId),
                Filter.Ne(x => x.IsDeleted, true)
            };

            if (ids != null && ids.Count > 0)
            {
                filters.Add(
                    Filter.Or(
                        Filter.AnyIn(x => x.ReferencedIds, ids),
                        Filter.In(x => x.Id, ids)));
            }

            if (query?.Filter != null)
            {
                filters.Add(query.Filter.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }
    }
}
