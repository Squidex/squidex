﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations.MongoDb;

public sealed class DeleteContentCollections(IMongoDatabase database) : IMigration
{
    public async Task UpdateAsync(
        CancellationToken ct)
    {
        await database.DropCollectionAsync("States_Contents", ct);
        await database.DropCollectionAsync("States_Contents_Archive", ct);
        await database.DropCollectionAsync("State_Content_Draft", ct);
        await database.DropCollectionAsync("State_Content_Published", ct);
    }
}
