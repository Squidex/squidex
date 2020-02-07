// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryContentsByQuery : OperationBase
    {
        private readonly IJsonSerializer serializer;
        private readonly ITextIndexer indexer;

        public QueryContentsByQuery(IJsonSerializer serializer, ITextIndexer indexer)
        {
            this.serializer = serializer;

            this.indexer = indexer;
        }

        protected override Task PrepareAsync(CancellationToken ct = default)
        {
            var index =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.Status)
                    .Descending(x => x.LastModified));

            return Collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
        }

        public async Task<IResultList<IContentEntity>> DoAsync(IAppEntity app, ISchemaEntity schema, ClrQuery query, Status[]? status, bool inDraft)
        {
            Guard.NotNull(app);
            Guard.NotNull(schema);
            Guard.NotNull(query);

            try
            {
                query = query.AdjustToModel(schema.SchemaDef);

                List<Guid>? fullTextIds = null;

                if (!string.IsNullOrWhiteSpace(query.FullText))
                {
                    fullTextIds = await indexer.SearchAsync(query.FullText, app, schema.Id, inDraft ? SearchScope.All : SearchScope.Published);

                    if (fullTextIds?.Count == 0)
                    {
                        return ResultList.CreateFrom<IContentEntity>(0);
                    }
                }

                var filter = CreateFilter(schema.Id, fullTextIds, status, query);

                var contentCount = Collection.Find(filter).CountDocumentsAsync();
                var contentItems =
                    Collection.Find(filter)
                        .QueryLimit(query)
                        .QuerySkip(query)
                        .QuerySort(query)
                        .ToListAsync();

                await Task.WhenAll(contentItems, contentCount);

                foreach (var entity in contentItems.Result)
                {
                    entity.ParseData(schema.SchemaDef, serializer);
                }

                return ResultList.Create<IContentEntity>(contentCount.Result, contentItems.Result);
            }
            catch (MongoQueryException ex)
            {
                if (ex.Message.Contains("17406"))
                {
                    throw new DomainException("Result set is too large to be retrieved. Use $top parameter to reduce the number of items.");
                }
                else
                {
                    throw;
                }
            }
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(Guid schemaId, ICollection<Guid>? ids, Status[]? status, ClrQuery? query)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedSchemaId, schemaId),
                Filter.Ne(x => x.IsDeleted, true)
            };

            if (status != null)
            {
                filters.Add(Filter.In(x => x.Status, status));
            }

            if (ids != null && ids.Count > 0)
            {
                filters.Add(Filter.In(x => x.Id, ids));
            }

            if (query?.Filter != null)
            {
                filters.Add(query.Filter.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }
    }
}
