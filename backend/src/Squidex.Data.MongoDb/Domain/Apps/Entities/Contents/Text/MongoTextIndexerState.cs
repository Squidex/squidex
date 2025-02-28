﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.MongoDb;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class MongoTextIndexerState(
    IMongoDatabase database,
    IContentRepository contentRepository)
    : MongoRepositoryBase<TextContentState>(database), ITextIndexerState, IDeleter
{
    static MongoTextIndexerState()
    {
        BsonUniqueContentIdSerializer.Register();

        BsonClassMap.TryRegisterClassMap<TextContentState>(cm =>
        {
            cm.MapIdProperty(x => x.UniqueContentId);

            cm.MapProperty(x => x.State)
                .SetElementName("s");
        });
    }

    int IDeleter.Order => -2000;

    protected override string CollectionName()
    {
        return "TextIndexerState";
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        var ids =
            contentRepository.StreamIds(app.Id, null, SearchScope.All, ct)
                .Select(x => new UniqueContentId(app.Id, x));

        await DeleteInBatchesAsync(ids, ct);
    }

    async Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        var ids =
            contentRepository.StreamIds(app.Id, [schema.Id], SearchScope.All, ct)
                .Select(x => new UniqueContentId(app.Id, x));

        await DeleteInBatchesAsync(ids, ct);
    }

    private async Task DeleteInBatchesAsync(IAsyncEnumerable<UniqueContentId> ids,
        CancellationToken ct)
    {
        await foreach (var batch in ids.Batch(1000, ct).WithCancellation(ct))
        {
            var filter = Filter.In(x => x.UniqueContentId, batch);

            await Collection.DeleteManyAsync(filter, ct);
        }
    }

    public async Task<Dictionary<UniqueContentId, TextContentState>> GetAsync(HashSet<UniqueContentId> ids,
        CancellationToken ct = default)
    {
        var entities = await Collection.Find(Filter.In(x => x.UniqueContentId, ids)).ToListAsync(ct);

        return entities.ToDictionary(x => x.UniqueContentId);
    }

    public Task SetAsync(List<TextContentState> updates,
        CancellationToken ct = default)
    {
        var writes = new List<WriteModel<TextContentState>>();

        foreach (var update in updates)
        {
            if (update.State == TextState.Deleted)
            {
                writes.Add(
                    new DeleteOneModel<TextContentState>(
                        Filter.Eq(x => x.UniqueContentId, update.UniqueContentId)));
            }
            else
            {
                writes.Add(
                    new ReplaceOneModel<TextContentState>(
                        Filter.Eq(x => x.UniqueContentId, update.UniqueContentId), update)
                    {
                        IsUpsert = true,
                    });
            }
        }

        if (writes.Count == 0)
        {
            return Task.CompletedTask;
        }

        return Collection.BulkWriteAsync(writes, BulkUnordered, ct);
    }
}
