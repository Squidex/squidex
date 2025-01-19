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

namespace Squidex.Migrations.MongoDb;

public sealed class RestructureContentCollection(IMongoDatabase contentDatabase) : MongoBase<BsonDocument>, IMigration
{
    public async Task UpdateAsync(
        CancellationToken ct)
    {
        if (await contentDatabase.CollectionExistsAsync("State_Content_Draft", ct))
        {
            await contentDatabase.DropCollectionAsync("State_Contents", ct);
            await contentDatabase.DropCollectionAsync("State_Content_Published", ct);

            await contentDatabase.RenameCollectionAsync("State_Content_Draft", "State_Contents", cancellationToken: ct);
        }

        if (await contentDatabase.CollectionExistsAsync("State_Contents", ct))
        {
            var collection = contentDatabase.GetCollection<BsonDocument>("State_Contents");

            await collection.UpdateManyAsync(FindAll, Update.Unset("dt"), cancellationToken: ct);
        }
    }
}
