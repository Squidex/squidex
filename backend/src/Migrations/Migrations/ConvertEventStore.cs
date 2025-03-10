﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Events;
using Squidex.Events.Mongo;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations;

public sealed class ConvertEventStore(IEventStore eventStore) : MongoBase<BsonDocument>, IMigration
{
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
                foreach (BsonDocument @event in commit["Events"].AsBsonArray.OfType<BsonDocument>())
                {
                    var meta = BsonDocument.Parse(@event["Metadata"].AsString);

                    @event.Remove("EventId");
                    @event["Metadata"] = meta;
                }

                await WriteAsync(new ReplaceOneModel<BsonDocument>(Filter.Eq("_id", commit["_id"].AsString), commit), false);
            }, ct);

            await WriteAsync(null, true);
        }
    }
}
