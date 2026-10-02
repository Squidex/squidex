// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Squidex.Migrations;

public sealed class DropOldAssetIndex(IMongoDatabase database) : IMigration
{
    // The old index started with the sort fields and was therefore not bounded by the app.
    public const string IndexName = "mt_-1_id_1__ai_1_dl_1_pi_1_td_1";

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        // Each asset shard has its own collection.
        var collectionNames = await (await database.ListCollectionNamesAsync(cancellationToken: ct)).ToListAsync(ct);

        foreach (var collectionName in collectionNames.Where(x => x.StartsWith("States_Assets2", StringComparison.Ordinal)))
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            try
            {
                await collection.Indexes.DropOneAsync(IndexName, ct);
            }
            catch (MongoCommandException ex) when (ex.Code == 27)
            {
                // The index does not exist, for example in the count collections.
            }
        }
    }
}
