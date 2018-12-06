// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations
{
    public sealed class ConvertRuleEventsJson : IMigration
    {
        private readonly IMongoCollection<BsonDocument> collection;

        public ConvertRuleEventsJson(IMongoDatabase database)
        {
            collection = database.GetCollection<BsonDocument>("RuleEvents");
        }

        public async Task UpdateAsync()
        {
            foreach (var document in collection.Find(new BsonDocument()).ToEnumerable())
            {
                document["Job"]["actionData"] = document["Job"]["actionData"].ToBsonDocument().ToJson();

                await collection.ReplaceOneAsync(Builders<BsonDocument>.Filter.Eq("_id", document["_id"].ToString()), document);
            }
        }
    }
}
