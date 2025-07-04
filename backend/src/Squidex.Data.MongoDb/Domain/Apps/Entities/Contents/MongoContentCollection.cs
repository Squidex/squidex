﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Operations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Counts;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class MongoContentCollection : MongoRepositoryBase<MongoContentEntity>
{
    private readonly QueryAsStream queryAsStream;
    private readonly QueryById queryBdId;
    private readonly QueryByIds queryByIds;
    private readonly QueryByQuery queryByQuery;
    private readonly QueryInDedicatedCollection? queryInDedicatedCollection;
    private readonly QueryReferences queryReferences;
    private readonly QueryReferrers queryReferrers;
    private readonly QueryScheduled queryScheduled;
    private readonly ReadPreference readPreference;
    private readonly string name;

    public MongoContentCollection(string name, IMongoDatabase database, ILogger log, ReadPreference readPreference,
        bool dedicatedCollections)
        : base(database)
    {
        this.name = name;

        queryAsStream = new QueryAsStream();
        queryBdId = new QueryById();
        queryByIds = new QueryByIds();
        queryReferences = new QueryReferences(queryByIds);
        queryReferrers = new QueryReferrers();
        queryScheduled = new QueryScheduled();
        queryByQuery = new QueryByQuery(new MongoCountCollection(database, log, name));

        if (dedicatedCollections)
        {
            queryInDedicatedCollection =
                new QueryInDedicatedCollection(
                    database.Client,
                    database.DatabaseNamespace.DatabaseName,
                    name);
        }

        this.readPreference = readPreference;
    }

    protected override string CollectionName()
    {
        return name;
    }

    protected override MongoCollectionSettings CollectionSettings()
    {
        return new MongoCollectionSettings
        {
            ReadPreference = readPreference,
        };
    }

    protected override Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection,
        CancellationToken ct)
    {
        var operations = new OperationBase[]
        {
            queryAsStream,
            queryBdId,
            queryByIds,
            queryByQuery,
            queryReferences,
            queryReferrers,
            queryScheduled,
        };

        foreach (var operation in operations)
        {
            operation.Setup(collection);
        }

        return collection.Indexes.CreateManyAsync(operations.SelectMany(x => x.CreateIndexes()), ct);
    }

    public Task ResetScheduledAsync(DomainId appId, DomainId id,
        CancellationToken ct)
    {
        var documentId = DomainId.Combine(appId, id);

        return Collection.UpdateOneAsync(
            x => x.DocumentId == documentId,
            Update
                .Unset(x => x.ScheduleJob)
                .Unset(x => x.ScheduledAt),
            cancellationToken: ct);
    }

    public IAsyncEnumerable<Content> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds,
        CancellationToken ct)
    {
        return queryAsStream.StreamAll(appId, schemaIds, ct);
    }

    public IAsyncEnumerable<Content> StreamReferencing(DomainId appId, DomainId reference, int take,
        CancellationToken ct)
    {
        return queryReferrers.StreamReferencing(appId, reference, take, ct);
    }

    public IAsyncEnumerable<Content> QueryScheduledWithoutDataAsync(Instant now,
        CancellationToken ct)
    {
        return queryScheduled.QueryAsync(now, ct);
    }

    public IAsyncEnumerable<DomainId> StreamIds(DomainId appId, HashSet<DomainId>? schemaIds,
        CancellationToken ct)
    {
        return queryAsStream.StreamAllIds(appId, schemaIds, ct);
    }

    public async Task DeleteAppAsync(DomainId appId,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentCollection/DeleteAppAsync"))
        {
            await Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, appId), ct);
        }
    }

    public async Task DeleteSchemaAsync(DomainId schemaId,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentCollection/DeleteSchemaAsync"))
        {
            await Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedSchemaId, schemaId), ct);
        }
    }

    public Task<IResultList<Content>> QueryAsync(App app, Schema schema, Q q,
        CancellationToken ct)
    {
        return QueryAsync(app, [schema], true, q, ct);
    }

    public Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, Q q,
        CancellationToken ct)
    {
        return QueryAsync(app, schemas, false, q, ct);
    }

    private async Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, bool isSingle, Q q,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryAsync"))
        {
            try
            {
                if (q.Ids is { Count: > 0 } && schemas.Count > 0)
                {
                    return await queryByIds.QueryAsync(app, schemas, q, ct);
                }

                if (q.ScheduledFrom != null && q.ScheduledTo != null && schemas.Count > 0)
                {
                    return await queryScheduled.QueryAsync(app, schemas, q, ct);
                }

                if (q.Referencing != default && schemas.Count > 0)
                {
                    return await queryReferences.QueryAsync(app, schemas, q, ct);
                }

                if (q.Reference != default && schemas.Count > 0)
                {
                    return await queryByQuery.QueryAsync(app, schemas, q, ct);
                }

                if (isSingle && queryInDedicatedCollection != null)
                {
                    return await queryInDedicatedCollection.QueryAsync(schemas.Single(), q, ct);
                }

                if (isSingle)
                {
                    return await queryByQuery.QueryAsync(schemas.Single(), q, ct);
                }

                return ResultList.Empty<Content>();
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
    }

    public async Task<Content?> FindContentAsync(Schema schema, DomainId id, IReadOnlySet<string>? fields,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentCollection/FindContentAsync"))
        {
            return await queryBdId.QueryAsync(schema, id, fields, ct);
        }
    }

    public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, HashSet<DomainId> ids,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryIdsAsync"))
        {
            return await queryByIds.QueryIdsAsync(app, ids, ct);
        }
    }

    public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, Schema schema, FilterNode<ClrValue> filterNode,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentCollection/QueryIdsAsync"))
        {
            return await queryByQuery.QueryIdsAsync(app, schema, filterNode, ct);
        }
    }

    public async Task<bool> HasReferrersAsync(App app, DomainId reference,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("MongoContentCollection/HasReferrersAsync"))
        {
            return await queryReferrers.CheckExistsAsync(app, reference, ct);
        }
    }

    public Task<MongoContentEntity> FindAsync(DomainId documentId,
        CancellationToken ct = default)
    {
        return Collection.Find(x => x.DocumentId == documentId).FirstOrDefaultAsync(ct);
    }

    public IAsyncEnumerable<MongoContentEntity> StreamAll(
        CancellationToken ct)
    {
        return Collection.Find(FindAll).ToAsyncEnumerable(ct);
    }

    public async Task UpsertAsync(SnapshotWriteJob<MongoContentEntity> job,
        CancellationToken ct = default)
    {
        if (queryInDedicatedCollection != null)
        {
            await queryInDedicatedCollection.UpsertAsync(job, ct);
        }

        await Collection.ReplaceOneAsync(Filter.Eq(x => x.DocumentId, job.Key), job.Value, UpsertReplace, ct);
    }

    public async Task UpsertVersionedAsync(IClientSessionHandle session, SnapshotWriteJob<MongoContentEntity> job,
        CancellationToken ct = default)
    {
        if (queryInDedicatedCollection != null)
        {
            await queryInDedicatedCollection.UpsertVersionedAsync(session, job, ct);
        }

        await Collection.UpsertVersionedAsync(session, job, Field.Of<MongoContentEntity>(x => nameof(x.Version)), ct);
    }

    public async Task RemoveAsync(DomainId key,
        CancellationToken ct = default)
    {
        var previous = await Collection.FindOneAndDeleteAsync(x => x.DocumentId == key, null, ct);

        if (queryInDedicatedCollection != null && previous != null)
        {
            await queryInDedicatedCollection.RemoveAsync(previous, ct);
        }
    }

    public async Task RemoveAsync(IClientSessionHandle session, DomainId key,
        CancellationToken ct = default)
    {
        var previous = await Collection.FindOneAndDeleteAsync(session, x => x.DocumentId == key, null, ct);

        if (queryInDedicatedCollection != null && previous != null)
        {
            await queryInDedicatedCollection.RemoveAsync(session, previous, ct);
        }
    }

    public async Task AddToCollectionsAsync(MongoContentEntity entity, Action<IMongoCollection<MongoContentEntity>, MongoContentEntity> add,
        CancellationToken ct)
    {
        if (queryInDedicatedCollection != null)
        {
            add(await queryInDedicatedCollection.GetCollectionAsync(entity.AppId.Id, entity.SchemaId.Id), entity);
        }

        add(Collection, entity);
    }

    public async Task CreateIndexAsync(DomainId appId, DomainId schemaId, IndexDefinition index,
        CancellationToken ct = default)
    {
        if (queryInDedicatedCollection != null)
        {
            await queryInDedicatedCollection.CreateIndexAsync(appId, schemaId, index, ct);
        }
    }

    public async Task DropIndexAsync(DomainId appId, DomainId schemaId, string name,
        CancellationToken ct = default)
    {
        if (queryInDedicatedCollection != null)
        {
            await queryInDedicatedCollection.DropIndexAsync(appId, schemaId, name, ct);
        }
    }

    public async Task<List<IndexDefinition>> GetIndexesAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct = default)
    {
        if (queryInDedicatedCollection != null)
        {
            return await queryInDedicatedCollection.GetIndexesAsync(appId, schemaId, ct);
        }

        return [];
    }
}
