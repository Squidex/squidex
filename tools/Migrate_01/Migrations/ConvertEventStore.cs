// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;

namespace Migrate_01.Migrations
{
    public sealed class ConvertEventStore : IMigration
    {
        private readonly IEventStore eventStore;

        public ConvertEventStore(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task UpdateAsync()
        {
            if (eventStore is MongoEventStore mongoEventStore)
            {
                var collection = mongoEventStore.RawCollection;

                var filter = Builders<BsonDocument>.Filter;

                var writesBatches = new List<WriteModel<BsonDocument>>();

                async Task WriteAsync(WriteModel<BsonDocument> model, bool force)
                {
                    if (model != null)
                    {
                        writesBatches.Add(model);
                    }

                    if (writesBatches.Count == 1000 || (force && writesBatches.Count > 0))
                    {
                        await collection.BulkWriteAsync(writesBatches);

                        writesBatches.Clear();
                    }
                }

                await collection.Find(new BsonDocument()).ForEachAsync(async commit =>
                {
                    foreach (BsonDocument @event in commit["Events"].AsBsonArray)
                    {
                        var meta = JObject.Parse(@event["Metadata"].AsString);

                        @event.Remove("EventId");
                        @event["Metadata"] = meta.ToBson();
                    }

                    await WriteAsync(new ReplaceOneModel<BsonDocument>(filter.Eq("_id", commit["_id"].AsString), commit), false);
                });

                await WriteAsync(null, true);
            }
        }
    }
}
