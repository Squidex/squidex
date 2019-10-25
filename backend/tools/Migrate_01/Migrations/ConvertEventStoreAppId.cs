﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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

                var filterer = Builders<BsonDocument>.Filter;
                var updater = Builders<BsonDocument>.Update;

                var writesBatches = new List<WriteModel<BsonDocument>>();

                async Task WriteAsync(WriteModel<BsonDocument>? model, bool force)
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
                    UpdateDefinition<BsonDocument>? update = null;

                    var index = 0;

                    foreach (BsonDocument @event in commit["Events"].AsBsonArray)
                    {
                        var data = JObject.Parse(@event["Payload"].AsString);

                        if (data.TryGetValue("appId", out var appIdValue))
                        {
                            var appId = NamedId<Guid>.Parse(appIdValue.ToString(), Guid.TryParse).Id.ToString();

                            var eventUpdate = updater.Set($"Events.{index}.Metadata.{SquidexHeaders.AppId}", appId);

                            if (update != null)
                            {
                                update = updater.Combine(update, eventUpdate);
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
                        var write = new UpdateOneModel<BsonDocument>(filterer.Eq("_id", commit["_id"].AsString), update);

                        await WriteAsync(write, false);
                    }
                });

                await WriteAsync(null, true);
            }
        }
    }
}
