// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations
{
    public sealed class ConvertEventStoreAppId : IMigration
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
                var collection = mongoEventStore.RawCollection;

                var filter = Builders<BsonDocument>.Filter;

                var updates = Builders<BsonDocument>.Update;

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
                    UpdateDefinition<BsonDocument>? update = null;

                    var index = 0;

                    foreach (BsonDocument @event in commit["Events"].AsBsonArray)
                    {
                        var data = BsonDocument.Parse(@event["Payload"].AsString);

                        if (data.TryGetValue("appId", out var appIdValue))
                        {
                            var appId = NamedId<Guid>.Parse(appIdValue.AsString, Guid.TryParse).Id.ToString();

                            var eventUpdate = updates.Set($"Events.{index}.Metadata.AppId", appId);

                            if (update != null)
                            {
                                update = updates.Combine(update, eventUpdate);
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
                        var write = new UpdateOneModel<BsonDocument>(filter.Eq("_id", commit["_id"].AsString), update);

                        await WriteAsync(write, false);
                    }
                }, ct);

                await WriteAsync(null, true);
            }
        }
    }
}
