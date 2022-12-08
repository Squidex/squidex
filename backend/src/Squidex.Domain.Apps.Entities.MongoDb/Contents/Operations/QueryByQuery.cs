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
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

internal sealed class QueryByQuery : OperationBase
{
    private readonly MongoCountCollection countCollection;

    public QueryByQuery(MongoCountCollection countCollection)
    {
        this.countCollection = countCollection;
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

    public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode,
        CancellationToken ct)
    {
        // We need to translate the query names to the document field names in MongoDB.
        var adjustedFilter = filterNode.AdjustToModel(appId);

        var filter = BuildFilter(appId, schemaId, adjustedFilter);

        var contentEntities = await Collection.FindStatusAsync(filter, ct);
        var contentResults = contentEntities.Select(x => new ContentIdStatus(x.IndexedSchemaId, x.Id, x.Status)).ToList();

        return contentResults;
    }

    public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q,
        CancellationToken ct)
    {
        // We need to translate the query names to the document field names in MongoDB.
        var query = q.Query.AdjustToModel(app.Id);

        var (filter, isDefault) = CreateFilter(app.Id, schemas.Select(x => x.Id), query, q.Reference, q.CreatedBy);

        var contentEntities = await Collection.QueryContentsAsync(filter, query, ct);
        var contentTotal = (long)contentEntities.Count;

        if (contentTotal >= query.Take || query.Skip > 0)
        {
            if (q.NoTotal || (q.NoSlowTotal && query.Filter != null))
            {
                contentTotal = -1;
            }
            else if (query.IsSatisfiedByIndex())
            {
                // It is faster to filter with sorting when there is an index, because it forces the index to be used.
                contentTotal = await Collection.Find(filter).QuerySort(query).CountDocumentsAsync(ct);
            }
            else
            {
                contentTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }
        }

        return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
    }

    public async Task<IResultList<IContentEntity>> QueryAsync(ISchemaEntity schema, Q q,
        CancellationToken ct)
    {
        // We need to translate the query names to the document field names in MongoDB.
        var query = q.Query.AdjustToModel(schema.AppId.Id);

        // Default means that no other filters are applied and we only query by app and schema.
        var (filter, isDefault) = CreateFilter(schema.AppId.Id, Enumerable.Repeat(schema.Id, 1), query, q.Reference, q.CreatedBy);

        var contentEntities = await Collection.QueryContentsAsync(filter, query, ct);
        var contentTotal = (long)contentEntities.Count;

        if (contentTotal >= query.Take || query.Skip > 0)
        {
            if (q.NoTotal || (q.NoSlowTotal && query.Filter != null))
            {
                contentTotal = -1;
            }
            else if (isDefault)
            {
                // Cache total count by app and schema because no other filters are applied (aka default).
                var totalKey = $"{schema.AppId.Id}_{schema.Id}";

                contentTotal = await countCollection.GetOrAddAsync(totalKey, ct => Collection.Find(filter).CountDocumentsAsync(ct), ct);
            }
            else if (query.IsSatisfiedByIndex())
            {
                // It is faster to filter with sorting when there is an index, because it forces the index to be used.
                contentTotal = await Collection.Find(filter).QuerySort(query).CountDocumentsAsync(ct);
            }
            else
            {
                contentTotal = await Collection.Find(filter).CountDocumentsAsync(ct);
            }
        }

        return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
    }

    private static FilterDefinition<MongoContentEntity> BuildFilter(DomainId appId, DomainId schemaId,
        FilterNode<ClrValue>? filter)
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

    private static (FilterDefinition<MongoContentEntity>, bool) CreateFilter(DomainId appId, IEnumerable<DomainId> schemaIds, ClrQuery? query,
        DomainId referenced, RefToken? createdBy)
    {
        var filters = new List<FilterDefinition<MongoContentEntity>>
        {
            Filter.Gt(x => x.LastModified, default),
            Filter.Gt(x => x.Id, default),
            Filter.Eq(x => x.IndexedAppId, appId),
            Filter.In(x => x.IndexedSchemaId, schemaIds)
        };

        var isDefault = false;

        if (query?.HasFilterField("dl") != true)
        {
            filters.Add(Filter.Ne(x => x.IsDeleted, true));

            isDefault = true;
        }

        if (query?.Filter != null)
        {
            filters.Add(query.Filter.BuildFilter<MongoContentEntity>());

            isDefault = false;
        }

        if (referenced != default)
        {
            filters.Add(Filter.AnyEq(x => x.ReferencedIds, referenced));

            isDefault = false;
        }

        if (createdBy != null)
        {
            filters.Add(Filter.Eq(x => x.CreatedBy, createdBy));

            isDefault = false;
        }

        return (Filter.And(filters), isDefault);
    }
}
