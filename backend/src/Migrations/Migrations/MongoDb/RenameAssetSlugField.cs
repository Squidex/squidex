// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrations.Migrations.MongoDb
{
    public sealed class RenameAssetSlugField : IMigration
    {
        private readonly IMongoDatabase database;

        public RenameAssetSlugField(IMongoDatabase database)
        {
            this.database = database;
        }

        public Task UpdateAsync(
            CancellationToken ct)
        {
            var collection = database.GetCollection<BsonDocument>("States_Assets");

            var update = Builders<BsonDocument>.Update.Rename("FileNameSlug", "Slug");

            return collection.UpdateManyAsync(new BsonDocument(), update, cancellationToken: ct);
        }
    }
}
