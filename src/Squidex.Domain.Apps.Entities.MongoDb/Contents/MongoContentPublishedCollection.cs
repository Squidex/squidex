// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.MongoDb;

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
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoContentEntity>(Index.Text(x => x.DataText).Ascending(x => x.IndexedSchemaId)));

            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoContentEntity>(
                    Index
                        .Ascending(x => x.IndexedSchemaId)
                        .Ascending(x => x.Id)));

            await base.SetupCollectionAsync(collection);
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id)
        {
            var contentEntity =
                await Collection.Find(x => x.IndexedSchemaId == schema.Id && x.Id == id).Not(x => x.DataText)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }

        public Task UpsertAsync(MongoContentEntity content)
        {
            content.DataText = content.DataByIds.ToFullText();
            content.DataDraftByIds = null;
            content.ScheduleJob = null;
            content.ScheduledAt = null;

            return Collection.ReplaceOneAsync(x => x.Id == content.Id, content, new UpdateOptions { IsUpsert = true });
        }

        public Task RemoveAsync(Guid id)
        {
            return Collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
