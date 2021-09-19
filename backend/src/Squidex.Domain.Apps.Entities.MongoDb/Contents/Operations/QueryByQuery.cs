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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryByQuery : OperationBase
    {
        private readonly IAppProvider appProvider;

        [BsonIgnoreExtraElements]
        internal sealed class IdOnly
        {
            [BsonId]
            [BsonElement("_id")]
            public DomainId Id { get; set; }

            public MongoContentEntity[] Joined { get; set; }
        }

        public QueryByQuery(IAppProvider appProvider)
        {
            this.appProvider = appProvider;
        }

        public override IEnumerable<CreateIndexModel<MongoContentEntity>> CreateIndexes()
        {
            yield return new CreateIndexModel<MongoContentEntity>(Index
                .Descending(x => x.LastModified)
                .Ascending(x => x.Id)
                .Ascending(x => x.IndexedAppId)
                .Ascending(x => x.IndexedSchemaId)
                .Ascending(x => x.IsDeleted)
                .Ascending(x => x.ReferencedIds));

            yield return new CreateIndexModel<MongoContentEntity>(Index
                .Ascending(x => x.IndexedSchemaId)
                .Ascending(x => x.IsDeleted)
                .Descending(x => x.LastModified));
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode,
            CancellationToken ct)
        {
            Guard.NotNull(filterNode, nameof(filterNode));

            try
            {
                var schema = await appProvider.GetSchemaAsync(appId, schemaId, ct: ct);

                if (schema == null)
                {
                    return new List<(DomainId SchemaId, DomainId Id, Status Status)>();
                }

                var filter = BuildFilter(appId, schemaId, filterNode.AdjustToModel(appId));

                var contentItems = await Collection.FindStatusAsync(filter, ct);

                return contentItems.Select(x => (x.IndexedSchemaId, x.Id, x.Status)).ToList();
            }
            catch (MongoCommandException ex) when (ex.Code == 96)
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
            catch (MongoQueryException ex) when (ex.Message.Contains("17406", StringComparison.Ordinal))
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q,
            CancellationToken ct)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(q, nameof(q));

            try
            {
                var query = q.Query.AdjustToModel(app.Id);

                var filter = CreateFilter(app.Id, schemas.Select(x => x.Id), query, q.Reference, q.CreatedBy);

                var contentEntities = await FindContentsAsync(query, filter, ct);
                var contentTotal = (long)contentEntities.Count;

                if (q.NoTotal)
                {
                    contentTotal = -1;
                }
                else if (contentTotal >= q.Query.Take || q.Query.Skip > 0)
                {
                    contentTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                }

                return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
            }
            catch (MongoCommandException ex) when (ex.Code == 96)
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
            catch (MongoQueryException ex) when (ex.Message.Contains("17406", StringComparison.Ordinal))
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q,
            CancellationToken ct)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(q, nameof(q));

            try
            {
                var query = q.Query.AdjustToModel(app.Id);

                var filter = CreateFilter(schema.AppId.Id, Enumerable.Repeat(schema.Id, 1), query, q.Reference, q.CreatedBy);

                var contentEntities = await FindContentsAsync(query, filter, ct);
                var contentTotal = (long)contentEntities.Count;

                if (q.NoTotal)
                {
                    contentTotal = -1;
                }
                else if (contentTotal >= q.Query.Take || q.Query.Skip > 0)
                {
                    contentTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
                }

                return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
            }
            catch (MongoCommandException ex) when (ex.Code == 96)
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
            catch (MongoQueryException ex) when (ex.Message.Contains("17406", StringComparison.Ordinal))
            {
                throw new DomainException(T.Get("common.resultTooLarge"));
            }
        }

        private async Task<List<MongoContentEntity>> FindContentsAsync(ClrQuery query, FilterDefinition<MongoContentEntity> filter,
            CancellationToken ct)
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
                        .ToListAsync(ct);

                return joined.Select(x => x.Joined[0]).ToList();
            }

            var result =
                Collection.Find(filter)
                    .QuerySort(query)
                    .QueryLimit(query)
                    .QuerySkip(query)
                    .ToListAsync(ct);

            return await result;
        }

        private static bool IsSatisfiedByIndex(ClrQuery query)
        {
            return query.Sort != null &&
                query.Sort.Count == 2 &&
                query.Sort[0].Path.ToString() == "mt" &&
                query.Sort[0].Order == SortOrder.Descending &&
                query.Sort[1].Path.ToString() == "id" &&
                query.Sort[1].Order == SortOrder.Ascending;
        }

        private static FilterDefinition<MongoContentEntity> BuildFilter(DomainId appId, DomainId schemaId, FilterNode<ClrValue>? filter)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Exists(x => x.LastModified),
                Filter.Exists(x => x.Id),
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.Eq(x => x.IndexedSchemaId, schemaId)
            };

            if (filter?.HasField("dl") != true)
            {
                filters.Add(Filter.Ne(x => x.IsDeleted, true));
            }

            if (filter != null)
            {
                filters.Add(filter.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, IEnumerable<DomainId> schemaIds,  ClrQuery? query,
            DomainId referenced, RefToken? createdBy)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Exists(x => x.LastModified),
                Filter.Exists(x => x.Id),
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.In(x => x.IndexedSchemaId, schemaIds)
            };

            if (query?.HasFilterField("dl") != true)
            {
                filters.Add(Filter.Ne(x => x.IsDeleted, true));
            }

            if (query?.Filter != null)
            {
                filters.Add(query.Filter.BuildFilter<MongoContentEntity>());
            }

            if (referenced != default)
            {
                filters.Add(Filter.AnyEq(x => x.ReferencedIds, referenced));
            }

            if (createdBy != null)
            {
                filters.Add(Filter.Eq(x => x.CreatedBy, createdBy));
            }

            return Filter.And(filters);
        }
    }
}
