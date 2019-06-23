// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations.MongoDb
{
    public sealed class CreateStatusColors : IMigration
    {
        private readonly IMongoDatabase contentDatabase;

        public CreateStatusColors(IMongoDatabase contentDatabase)
        {
            this.contentDatabase = contentDatabase;
        }

        public async Task UpdateAsync()
        {
            var collection = contentDatabase.GetCollection<BsonDocument>("State_Contents");

            await collection.UpdateManyAsync(
                Builders<BsonDocument>.Filter.Eq("ss", "Archived"),
                Builders<BsonDocument>.Update.Set("sc", StatusColors.Archived));

            await collection.UpdateManyAsync(
                Builders<BsonDocument>.Filter.Eq("ss", "Draft"),
                Builders<BsonDocument>.Update.Set("sc", StatusColors.Draft));

            await collection.UpdateManyAsync(
                Builders<BsonDocument>.Filter.Eq("ss", "Published"),
                Builders<BsonDocument>.Update.Set("sc", StatusColors.Published));
        }
    }
}
