// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations
{
    public sealed class ConvertEventStore : IMigration
    {
        private readonly IEventStore eventStore;

        public ConvertEventStore(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task UpdateAsync(
            CancellationToken ct)
        {
            if (eventStore is MongoEventStore mongoEventStore)
            {
                var collection = mongoEventStore.RawCollection;

                var filter = Builders<BsonDocument>.Filter;

                var writes = new List<WriteModel<BsonDocument>>();
                var writeOptions = new BulkWriteOptions
                {
                    IsOrdered = false
                };

                async Task WriteAsync(WriteModel<BsonDocument>? model, bool force)
                {
                    if (model != null)
                    {
                        writes.Add(model);
                    }

                    if (writes.Count == 1000 || (force && writes.Count > 0))
                    {
                        await collection.BulkWriteAsync(writes, writeOptions, ct);

                        writes.Clear();
                    }
                }

                await collection.Find(new BsonDocument()).ForEachAsync(async commit =>
                {
                    foreach (BsonDocument @event in commit["Events"].AsBsonArray)
                    {
                        var meta = BsonDocument.Parse(@event["Metadata"].AsString);

                        @event.Remove("EventId");
                        @event["Metadata"] = meta;
                    }

                    await WriteAsync(new ReplaceOneModel<BsonDocument>(filter.Eq("_id", commit["_id"].AsString), commit), false);
                }, ct);

                await WriteAsync(null, true);
            }
        }
    }
}
