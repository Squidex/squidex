// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations.MongoDb
{
    public sealed class DeleteContentCollections : IMigration
    {
        private readonly IMongoDatabase database;

        public DeleteContentCollections(IMongoDatabase database)
        {
            this.database = database;
        }

        public async Task UpdateAsync(
            CancellationToken ct)
        {
            await database.DropCollectionAsync("States_Contents", ct);
            await database.DropCollectionAsync("States_Contents_Archive", ct);
            await database.DropCollectionAsync("State_Content_Draft", ct);
            await database.DropCollectionAsync("State_Content_Published", ct);
        }
    }
}
