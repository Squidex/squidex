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

namespace Migrations.Migrations.MongoDb;

public sealed class RenameAssetSlugField(IMongoDatabase database) : MongoBase<BsonDocument>, IMigration
{
    public Task UpdateAsync(
        CancellationToken ct)
    {
        // Do not resolve in constructor, because most of the time it is not executed anyway.
        var collection = database.GetCollection<BsonDocument>("States_Assets");

        var update = Builders<BsonDocument>.Update.Rename("FileNameSlug", "Slug");

        return collection.UpdateManyAsync(FindAll, update, cancellationToken: ct);
    }
}
