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

namespace Migrate_01.Migrations.MongoDb
{
    public sealed class RenameAssetMetadata : IMigration
    {
        private readonly IMongoDatabase database;

        public RenameAssetMetadata(IMongoDatabase database)
        {
            this.database = database;
        }

        public async Task UpdateAsync()
        {
            var collection = database.GetCollection<BsonDocument>("States_Assets");

            var update1 =
                Builders<BsonDocument>.Update
                    .Set("md", new BsonDocument());

            await collection.UpdateManyAsync(new BsonDocument(), update1);

            var update2 =
                Builders<BsonDocument>.Update
                    .Rename("ph", "md.PixelHeight")
                    .Rename("pw", "md.PixelWidth");

            await collection.UpdateManyAsync(new BsonDocument(), update2);
        }
    }
}
