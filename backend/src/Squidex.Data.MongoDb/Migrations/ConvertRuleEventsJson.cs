// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Migrations;

public sealed class ConvertRuleEventsJson(IMongoDatabase database) : MongoBase<BsonDocument>, IMigration
{
    private readonly IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("RuleEvents");

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        foreach (var document in collection.Find(FindAll).ToEnumerable(ct))
        {
            try
            {
                document["Job"]["actionData"] = document["Job"]["actionData"].ToBsonDocument().ToJson();

                var filter = Filter.Eq("_id", document["_id"].ToString());

                await collection.ReplaceOneAsync(filter, document, cancellationToken: ct);
            }
            catch
            {
                continue;
            }
        }
    }
}
