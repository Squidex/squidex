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
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01
{
    public sealed class Migration00_ConvertEventStore : IMigration
    {
        private readonly IEventStore eventStore;

        public int FromVersion { get; } = 0;

        public int ToVersion { get; } = 1;

        public Migration00_ConvertEventStore(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task UpdateAsync(IEnumerable<IMigration> previousMigrations)
        {
            if (eventStore is MongoEventStore mongoEventStore)
            {
                var collection = mongoEventStore.RawCollection;

                var filter = Builders<BsonDocument>.Filter;

                await collection.Find(new BsonDocument()).ForEachAsync(async commit =>
                {
                    foreach (BsonDocument @event in commit["Events"].AsBsonArray)
                    {
                        @event.Remove("EventId");

                        @event["Payload"] = BsonDocument.Parse(@event["Payload"].AsString);
                        @event["Metadata"] = BsonDocument.Parse(@event["Metadata"].AsString);
                    }

                    await collection.ReplaceOneAsync(filter.Eq("_id", commit["_id"].AsString), commit);
                });
            }
        }
    }
}
