// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

internal sealed class QueryInDedicatedCollection : MongoBase<MongoContentEntity>
{
    private readonly ConcurrentDictionary<(DomainId, DomainId), Task<IMongoCollection<MongoContentEntity>>> collections =
        new ConcurrentDictionary<(DomainId, DomainId), Task<IMongoCollection<MongoContentEntity>>>();

    private readonly IMongoClient mongoClient;
    private readonly string prefixDatabase;
    private readonly string prefixCollection;

    public QueryInDedicatedCollection(IMongoClient mongoClient, string prefixDatabase, string prefixCollection)
    {
        this.mongoClient = mongoClient;
        this.prefixDatabase = prefixDatabase;
        this.prefixCollection = prefixCollection;
    }

    public Task<IMongoCollection<MongoContentEntity>> GetCollectionAsync(DomainId appId, DomainId schemaId)
    {
#pragma warning disable MA0106 // Avoid closure by using an overload with the 'factoryArgument' parameter
        return collections.GetOrAdd((appId, schemaId), async key =>
        {
            var (appId, schemaId) = key;

            var schemaDatabase = mongoClient.GetDatabase($"{prefixDatabase}_{appId}");
            var schemaCollection = schemaDatabase.GetCollection<MongoContentEntity>($"{prefixCollection}_{schemaId}");

            await schemaCollection.Indexes.CreateManyAsync(
                new[]
                {
                    new CreateIndexModel<MongoContentEntity>(Index
                        .Descending(x => x.LastModified)
                        .Ascending(x => x.Id)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.ReferencedIds)),
                    new CreateIndexModel<MongoContentEntity>(Index
                        .Ascending(x => x.IndexedSchemaId)
                        .Ascending(x => x.IsDeleted)
                        .Descending(x => x.LastModified))
                });

            return schemaCollection;
        });
#pragma warning restore MA0106 // Avoid closure by using an overload with the 'factoryArgument' parameter
    }

    public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(IAppEntity app, ISchemaEntity schema, FilterNode<ClrValue> filterNode,
        CancellationToken ct)
    {
        // We need to translate the filter names to the document field names in MongoDB.
        var adjustedFilter = filterNode.AdjustToModel(app.Id);

        var filter = BuildFilter(adjustedFilter);

        var contentCollection = await GetCollectionAsync(schema.AppId.Id, schema.Id);
        var contentEntities = await contentCollection.FindStatusAsync(filter, ct);
        var contentResults = contentEntities.Select(x => new ContentIdStatus(x.IndexedSchemaId, x.Id, x.Status)).ToList();

        return contentResults;
    }

    public async Task<IResultList<IContentEntity>> QueryAsync(ISchemaEntity schema, Q q,
        CancellationToken ct)
    {
        // We need to translate the query names to the document field names in MongoDB.
        var query = q.Query.AdjustToModel(schema.AppId.Id);

        var filter = CreateFilter(query, q.Reference, q.CreatedBy);

        var contentCollection = await GetCollectionAsync(schema.AppId.Id, schema.Id);
        var contentEntities = await contentCollection.QueryContentsAsync(filter, query, ct);
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
                contentTotal = await contentCollection.Find(filter).QuerySort(query).CountDocumentsAsync(ct);
            }
            else
            {
                contentTotal = await contentCollection.Find(filter).CountDocumentsAsync(ct);
            }
        }

        return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
    }

    public async Task UpsertAsync(SnapshotWriteJob<MongoContentEntity> job,
        CancellationToken ct = default)
    {
        var collection = await GetCollectionAsync(job.Value.AppId.Id, job.Value.SchemaId.Id);

        await collection.ReplaceOneAsync(Filter.Eq(x => x.DocumentId, job.Key), job.Value, UpsertReplace, ct);
    }

    public async Task UpsertVersionedAsync(IClientSessionHandle session, SnapshotWriteJob<MongoContentEntity> job,
        CancellationToken ct = default)
    {
        var collection = await GetCollectionAsync(job.Value.AppId.Id, job.Value.SchemaId.Id);

        await collection.UpsertVersionedAsync(session, job, ct);
    }

    public async Task RemoveAsync(MongoContentEntity value,
        CancellationToken ct = default)
    {
        var collection = await GetCollectionAsync(value.AppId.Id, value.SchemaId.Id);

        await collection.DeleteOneAsync(x => x.DocumentId == value.DocumentId, null, ct);
    }

    public async Task RemoveAsync(IClientSessionHandle session, MongoContentEntity value,
        CancellationToken ct = default)
    {
        var collection = await GetCollectionAsync(value.AppId.Id, value.SchemaId.Id);

        await collection.DeleteOneAsync(session, x => x.DocumentId == value.DocumentId, null, ct);
    }

    private static FilterDefinition<MongoContentEntity> BuildFilter(FilterNode<ClrValue>? filter)
    {
        var filters = new List<FilterDefinition<MongoContentEntity>>
        {
            Filter.Exists(x => x.LastModified),
            Filter.Exists(x => x.Id)
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

    private static FilterDefinition<MongoContentEntity> CreateFilter(ClrQuery? query,
        DomainId referenced, RefToken? createdBy)
    {
        var filters = new List<FilterDefinition<MongoContentEntity>>
        {
            Filter.Gt(x => x.LastModified, default),
            Filter.Gt(x => x.Id, default)
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
