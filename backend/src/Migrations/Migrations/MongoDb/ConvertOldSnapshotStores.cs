// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations.MongoDb
{
    public sealed class ConvertOldSnapshotStores : IMigration
    {
        private readonly IMongoDatabase database;

        public ConvertOldSnapshotStores(IMongoDatabase database)
        {
            this.database = database;
        }

        public Task UpdateAsync(
            CancellationToken ct)
        {
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
                    .Select(x => x.UpdateManyAsync(filter, update, cancellationToken: ct)));
        }
    }
}
