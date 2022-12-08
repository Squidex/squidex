// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;

namespace Migrations.Migrations.MongoDb;

public sealed class ConvertOldSnapshotStores : MongoBase<BsonDocument>, IMigration
{
    private readonly IMongoDatabase database;

    public ConvertOldSnapshotStores(IMongoDatabase database)
    {
        this.database = database;
    }

    public Task UpdateAsync(
        CancellationToken ct)
    {
        // Do not resolve in constructor, because most of the time it is not executed anyway.
        var collections = new[]
        {
            "States_Apps",
            "States_Rules",
            "States_Schemas"
        }.Select(x => database.GetCollection<BsonDocument>(x));

        var update = Update.Rename("State", "Doc");

        return Task.WhenAll(collections.Select(x => x.UpdateManyAsync(FindAll, update, cancellationToken: ct)));
    }
}
