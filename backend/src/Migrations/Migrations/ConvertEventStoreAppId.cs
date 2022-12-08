// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;

namespace Migrations.Migrations;

public sealed class ConvertEventStoreAppId : MongoBase<BsonDocument>, IMigration
{
    private readonly IEventStore eventStore;

    public ConvertEventStoreAppId(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        if (eventStore is MongoEventStore mongoEventStore)
        {
            // Do not resolve in constructor, because most of the time it is not executed anyway.
            var collection = mongoEventStore.RawCollection;

            var writes = new List<WriteModel<BsonDocument>>();

            async Task WriteAsync(WriteModel<BsonDocument>? model, bool force)
            {
                if (model != null)
                {
                    writes.Add(model);
                }

                if (writes.Count == 1000 || (force && writes.Count > 0))
                {
                    await collection.BulkWriteAsync(writes, BulkUnordered, ct);
                    writes.Clear();
                }
            }

            await collection.Find(FindAll).ForEachAsync(async commit =>
            {
                UpdateDefinition<BsonDocument>? update = null;

                var index = 0;

                foreach (BsonDocument @event in commit["Events"].AsBsonArray.OfType<BsonDocument>())
                {
                    var data = BsonDocument.Parse(@event["Payload"].AsString);

                    if (data.TryGetValue("appId", out var appIdValue))
                    {
                        var appId = NamedId<Guid>.Parse(appIdValue.AsString, Guid.TryParse).Id.ToString();

                        var eventUpdate = Update.Set($"Events.{index}.Metadata.AppId", appId);

                        if (update != null)
                        {
                            update = Update.Combine(update, eventUpdate);
                        }
                        else
                        {
                            update = eventUpdate;
                        }
                    }

                    index++;
                }

                if (update != null)
                {
                    var write = new UpdateOneModel<BsonDocument>(Filter.Eq("_id", commit["_id"].AsString), update);

                    await WriteAsync(write, false);
                }
            }, ct);

            await WriteAsync(null, true);
        }
    }
}
