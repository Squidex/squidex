// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    internal sealed class MongoContentPublishedCollection : MongoContentCollection
    {
        public MongoContentPublishedCollection(IMongoDatabase database)
            : base(database, "State_Content_Published")
        {
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(Index.Text(x => x.DataText).Ascending(x => x.IndexedSchemaId));

            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.Id));

            await base.SetupCollectionAsync(collection);
        }

        public Task UpsertAsync(MongoContentEntity content)
        {
            content.DataText = content.DataByIds.ToFullText();
            content.DataDraftByIds = null;

            return Collection.ReplaceOneAsync(x => x.Id == content.Id, content, new UpdateOptions { IsUpsert = true });
        }

        public Task RemoveAsync(Guid id)
        {
            return Collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
