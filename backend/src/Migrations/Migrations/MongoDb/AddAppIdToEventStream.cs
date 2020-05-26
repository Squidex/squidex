// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations.MongoDb
{
    public sealed class AddAppIdToEventStream : IMigration
    {
        private readonly IMongoDatabase database;

        public AddAppIdToEventStream(IMongoDatabase database)
        {
            this.database = database;
        }

        public async Task UpdateAsync()
        {
            var collection = database.GetCollection<BsonDocument>("Events");

            await collection.Find(new BsonDocument()).ForEachAsync(async commit =>
            {
                string? appId = null;

                foreach (var @event in commit["Events"].AsBsonArray)
                {
                    var metadata = @event["Metadata"].AsBsonDocument;

                    if (metadata.TryGetValue("AppId", out var value))
                    {
                        appId = value.AsString;
                    }
                }

                if (appId != null)
                {
                    var eventStream = commit["EventStream"].AsString;

                    if (!eventStream.StartsWith("app-", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = eventStream.Split("-");

                        var newStreamName = $"{parts[0]}-{appId}-{string.Join("-", parts.Skip(1))}";

                        await collection.UpdateOneAsync(
                            Builders<BsonDocument>.Filter.Eq("_id", commit["_id"].AsString),
                            Builders<BsonDocument>.Update.Set("EventStream", newStreamName));
                    }
                }
            });
        }
    }
}
