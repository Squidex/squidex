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

public sealed class RenameAssetMetadata : MongoBase<BsonDocument>, IMigration
{
    private readonly IMongoDatabase database;

    public RenameAssetMetadata(IMongoDatabase database)
    {
        this.database = database;
    }

    public async Task UpdateAsync(
        CancellationToken ct)
    {
        // Do not resolve in constructor, because most of the time it is not executed anyway.
        var collection = database.GetCollection<BsonDocument>("States_Assets");

        // Create metadata.
        await collection.UpdateManyAsync(FindAll,
            Update.Set("md", new BsonDocument()),
            cancellationToken: ct);

        // Remove null pixel infos.
        await collection.UpdateManyAsync(new BsonDocument("ph", BsonValue.Create(null)),
            Update.Unset("ph").Unset("pw"),
            cancellationToken: ct);

        // Set pixel metadata.
        await collection.UpdateManyAsync(FindAll,
            Update.Rename("ph", "md.pixelHeight").Rename("pw", "md.pixelWidth"),
            cancellationToken: ct);

        // Set type to image.
        await collection.UpdateManyAsync(new BsonDocument("im", true),
            Update.Set("at", "Image"),
            cancellationToken: ct);

        // Set type to unknown.
        await collection.UpdateManyAsync(new BsonDocument("im", false),
            Update.Set("at", "Unknown"),
            cancellationToken: ct);

        // Remove IsImage.
        await collection.UpdateManyAsync(FindAll,
            Update.Unset("im"),
            cancellationToken: ct);
    }
}
