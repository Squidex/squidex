// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations
{
    public sealed class DeleteArchiveCollectionSetup : IMigration
    {
        private readonly IMongoDatabase database;

        public DeleteArchiveCollectionSetup(IMongoDatabase database)
        {
            this.database = database;
        }

        public async Task UpdateAsync()
        {
            var collection = database.GetCollection<BsonDocument>("States_Contents");

            await collection.Indexes.DropAllAsync();
            await collection.UpdateManyAsync(new BsonDocument(), Builders<BsonDocument>.Update.Unset("id"));
        }
    }
}
