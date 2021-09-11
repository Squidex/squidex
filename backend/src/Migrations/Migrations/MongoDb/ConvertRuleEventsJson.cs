// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations.MongoDb
{
    public sealed class ConvertRuleEventsJson : IMigration
    {
        private readonly IMongoCollection<BsonDocument> collection;

        public ConvertRuleEventsJson(IMongoDatabase database)
        {
            collection = database.GetCollection<BsonDocument>("RuleEvents");
        }

        public async Task UpdateAsync(
            CancellationToken ct)
        {
            foreach (var document in collection.Find(new BsonDocument()).ToEnumerable(ct))
            {
                try
                {
                    document["Job"]["actionData"] = document["Job"]["actionData"].ToBsonDocument().ToJson();

                    var filter = Builders<BsonDocument>.Filter.Eq("_id", document["_id"].ToString());

                    await collection.ReplaceOneAsync(filter, document, cancellationToken: ct);
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
