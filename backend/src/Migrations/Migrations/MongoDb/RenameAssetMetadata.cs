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

namespace Migrations.Migrations.MongoDb
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

            var createMetadata =
                Builders<BsonDocument>.Update
                    .Set("md", new BsonDocument());

            await collection.UpdateManyAsync(new BsonDocument(), createMetadata);

            var removeNullPixelInfos =
                Builders<BsonDocument>.Update
                    .Unset("ph")
                    .Unset("pw");

            await collection.UpdateManyAsync(new BsonDocument("ph", BsonValue.Create(null)), removeNullPixelInfos);

            var setPixelDimensions =
                Builders<BsonDocument>.Update
                    .Rename("ph", "md.pixelHeight")
                    .Rename("pw", "md.pixelWidth");

            await collection.UpdateManyAsync(new BsonDocument(), setPixelDimensions);

            var setTypeToImage =
                Builders<BsonDocument>.Update
                    .Set("at", "Image");

            await collection.UpdateManyAsync(new BsonDocument("im", true), setTypeToImage);

            var setTypeToUnknown =
                Builders<BsonDocument>.Update
                    .Set("at", "Unknown");

            await collection.UpdateManyAsync(new BsonDocument("im", false), setTypeToUnknown);

            var removeIsImage =
                Builders<BsonDocument>.Update
                    .Unset("im");

            await collection.UpdateManyAsync(new BsonDocument(), removeIsImage);
        }
    }
}
