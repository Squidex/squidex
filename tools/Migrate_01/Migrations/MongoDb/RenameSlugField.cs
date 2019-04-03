// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Migrate_01.Migrations.MongoDb
{
    public sealed class RenameSlugField : IMigration
    {
        private readonly IMongoDatabase database;
        private readonly MongoDbOptions options;

        public RenameSlugField(IMongoDatabase database, IOptions<MongoDbOptions> options)
        {
            this.database = database;
            this.options = options.Value;
        }

        public Task UpdateAsync()
        {
            if (options.IsCosmosDb)
            {
                return TaskHelper.Done;
            }

            var collection = database.GetCollection<BsonDocument>("States_Assets");

            var update = Builders<BsonDocument>.Update.Rename("FileNameSlug", "Slug");

            return collection.UpdateManyAsync(new BsonDocument(), update);
        }
    }
}
