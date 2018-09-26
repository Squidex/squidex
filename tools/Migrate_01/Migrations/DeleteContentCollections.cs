// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations
{
    public sealed class DeleteContentCollections : IMigration
    {
        private readonly IMongoDatabase database;

        public DeleteContentCollections(IMongoDatabase database)
        {
            this.database = database;
        }

        public async Task UpdateAsync()
        {
            await database.DropCollectionAsync("States_Contents");
            await database.DropCollectionAsync("States_Contents_Archive");
        }
    }
}
