// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class MongoTextIndexerState : MongoRepositoryBase<TextContentState>, ITextIndexerState, IDeleter
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

    public MongoTextIndexerState(IMongoDatabase database)
        : base(database)
    {
    }

    protected override string CollectionName()
    {
        return "TextIndexerState";
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        var filter =
            Filter.And(
                Filter.Gte(x => x.UniqueContentId, new UniqueContentId(app.Id, DomainId.Empty)),
                Filter.Lt(x => x.UniqueContentId, BsonUniqueContentIdSerializer.NextAppId(app.Id)));

        await Collection.DeleteManyAsync(filter, ct);
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
                            IsUpsert = true
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
