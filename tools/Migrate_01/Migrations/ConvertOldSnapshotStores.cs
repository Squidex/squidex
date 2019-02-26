// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Migrate_01.Migrations
{
    public sealed class ConvertOldSnapshotStores : IMigration
    {
        private readonly IMongoDatabase database;
        private readonly MongoDbOptions options;

        public ConvertOldSnapshotStores(IMongoDatabase database, IOptions<MongoDbOptions> options)
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

            var collections = new[]
            {
                "States_Apps",
                "States_Rules",
                "States_Schemas"
            };

            var update = Builders<BsonDocument>.Update.Rename("State", "Doc");

            var filter = new BsonDocument();

            return Task.WhenAll(
                collections
                    .Select(x => database.GetCollection<BsonDocument>(x))
                    .Select(x => x.UpdateManyAsync(filter, update)));
        }
    }
}
