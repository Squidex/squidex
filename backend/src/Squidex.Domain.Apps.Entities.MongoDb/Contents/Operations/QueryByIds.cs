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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryByIds : OperationBase
    {
        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
            CancellationToken ct)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<(DomainId SchemaId, DomainId Id, Status Status)>();
            }

            var filter = CreateFilter(appId, null, ids);

            var contentItems = await Collection.FindStatusAsync(filter, ct);

            return contentItems.Select(x => (x.IndexedSchemaId, x.Id, x.Status)).ToList();
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(DomainId appId, List<ISchemaEntity> schemas, Q q,
            CancellationToken ct)
        {
            Guard.NotNull(q, nameof(q));

            if (q.Ids == null || q.Ids.Count == 0)
            {
                return ResultList.CreateFrom<IContentEntity>(0);
            }

            var filter = CreateFilter(appId, schemas.Select(x => x.Id), q.Ids.ToHashSet());

            var contentEntities = await FindContentsAsync(q.Query, filter);
            var contentTotal = (long)contentEntities.Count;

            if (q.NoTotal)
            {
                contentTotal = -1;
            }
            else if (contentTotal >= q.Query.Take || q.Query.Skip > 0)
            {
                contentTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }

            return ResultList.Create(contentTotal, contentEntities);
        }

        private async Task<List<MongoContentEntity>> FindContentsAsync(ClrQuery query, FilterDefinition<MongoContentEntity> filter)
        {
            var result =
                Collection.Find(filter)
                    .QueryLimit(query)
                    .QuerySkip(query)
                    .ToListAsync();

            return await result;
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, IEnumerable<DomainId>? schemaIds, HashSet<DomainId> ids)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>();

            var documentIds = ids.Select(x => DomainId.Combine(appId, x)).ToList();

            if (documentIds.Count > 1)
            {
                filters.Add(
                    Filter.Or(
                        Filter.In(x => x.DocumentId, documentIds)));
            }
            else
            {
                var first = documentIds[0];

                filters.Add(
                    Filter.Or(
                        Filter.Eq(x => x.DocumentId, first)));
            }

            if (schemaIds != null)
            {
                filters.Add(Filter.In(x => x.IndexedSchemaId, schemaIds));
            }

            filters.Add(Filter.Ne(x => x.IsDeleted, true));

            return Filter.And(filters);
        }
    }
}
