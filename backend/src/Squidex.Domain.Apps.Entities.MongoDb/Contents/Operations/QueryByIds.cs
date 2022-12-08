// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

internal sealed class QueryByIds : OperationBase
{
    public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        if (ids == null || ids.Count == 0)
        {
            return ReadonlyList.Empty<ContentIdStatus>();
        }

        // Create a filter from the Ids and ensure that the content ids match to the app ID.
        var filter = CreateFilter(appId, null, ids, null);

        var contentEntities = await Collection.FindStatusAsync(filter, ct);

        return contentEntities.Select(x => new ContentIdStatus(x.IndexedSchemaId, x.Id, x.Status)).ToList();
    }

    public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q,
        CancellationToken ct)
    {
        if (q.Ids == null || q.Ids.Count == 0)
        {
            return ResultList.Empty<IContentEntity>();
        }

        // We need to translate the query names to the document field names in MongoDB.
        var query = q.Query.AdjustToModel(app.Id);

        // Create a filter from the Ids and ensure that the content ids match to the schema IDs.
        var filter = CreateFilter(app.Id, schemas.Select(x => x.Id), q.Ids.ToHashSet(), query.Filter);

        var contentEntities = await FindContentsAsync(query, filter, ct);
        var contentTotal = (long)contentEntities.Count;

        if (contentTotal >= query.Take || query.Skip > 0)
        {
            if (q.NoTotal)
            {
                contentTotal = -1;
            }
            else
            {
                contentTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }
        }

        return ResultList.Create(contentTotal, contentEntities);
    }

    private async Task<List<MongoContentEntity>> FindContentsAsync(ClrQuery query, FilterDefinition<MongoContentEntity> filter,
        CancellationToken ct)
    {
        var result =
            Collection.Find(filter)
                .QuerySort(query)
                .QuerySkip(query)
                .QueryLimit(query)
                .ToListRandomAsync(Collection, query.Random, ct);

        return await result;
    }

    private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, IEnumerable<DomainId>? schemaIds, HashSet<DomainId> ids,
        FilterNode<ClrValue>? filter)
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
            filters.Add(
                Filter.Or(
                    Filter.Eq(x => x.DocumentId, documentIds[0])));
        }

        if (schemaIds != null)
        {
            filters.Add(Filter.In(x => x.IndexedSchemaId, schemaIds));
        }

        filters.Add(Filter.Ne(x => x.IsDeleted, true));

        if (filter != null)
        {
            filters.Add(filter.BuildFilter<MongoContentEntity>());
        }

        return Filter.And(filters);
    }
}
