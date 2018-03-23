// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations
{
    public sealed class ConvertEventStoreAppId : IMigration
    {
        private readonly IEventStore eventStore;

        public ConvertEventStoreAppId(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task UpdateAsync()
        {
            if (eventStore is MongoEventStore mongoEventStore)
            {
                var collection = mongoEventStore.RawCollection;

                var filter = Builders<BsonDocument>.Filter;

                await collection.Find(new BsonDocument()).ForEachAsync(async commit =>
                {
                    foreach (BsonDocument @event in commit["Events"].AsBsonArray)
                    {
                        var data = JObject.Parse(@event["Payload"].AsString);

                        if (data.TryGetValue("appId", out var appId))
                        {
                            @event["Metadata"][SquidexHeaders.AppId] = NamedId<Guid>.Parse(appId.ToString(), Guid.TryParse).Id.ToString();
                        }
                    }

                    await collection.ReplaceOneAsync(filter.Eq("_id", commit["_id"].AsString), commit);
                });
            }
        }
    }
}
