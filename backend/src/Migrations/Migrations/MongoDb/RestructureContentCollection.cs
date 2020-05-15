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
using Squidex.Infrastructure.MongoDb;

namespace Migrations.Migrations.MongoDb
{
    public sealed class RestructureContentCollection : IMigration
    {
        private readonly IMongoDatabase contentDatabase;

        public RestructureContentCollection(IMongoDatabase contentDatabase)
        {
            this.contentDatabase = contentDatabase;
        }

        public async Task UpdateAsync()
        {
            if (await contentDatabase.CollectionExistsAsync("State_Content_Draft"))
            {
                await contentDatabase.DropCollectionAsync("State_Contents");
                await contentDatabase.DropCollectionAsync("State_Content_Published");
                await contentDatabase.RenameCollectionAsync("State_Content_Draft", "State_Contents");
            }

            if (await contentDatabase.CollectionExistsAsync("State_Contents"))
            {
                var collection = contentDatabase.GetCollection<BsonDocument>("State_Contents");

                await collection.UpdateManyAsync(new BsonDocument(), Builders<BsonDocument>.Update.Unset("dt"));
            }
        }
    }
}
