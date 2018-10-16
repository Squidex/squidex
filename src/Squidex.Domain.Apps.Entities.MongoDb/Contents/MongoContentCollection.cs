// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    internal class MongoContentCollection : MongoRepositoryBase<MongoContentEntity>
    {
        private readonly string collectionName;

        public MongoContentCollection(IMongoDatabase database, string collectionName)
            : base(database)
        {
            this.collectionName = collectionName;
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection, CancellationToken ct = default(CancellationToken))
        {
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoContentEntity>(Index.Ascending(x => x.ReferencedIds)), cancellationToken: ct);
        }

        protected override string CollectionName()
        {
            return collectionName;
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Query query, Status[] status = null, bool useDraft = false)
        {
            try
            {
                query = query.AdjustToModel(schema.SchemaDef, useDraft);

                var filter = FindExtensions.BuildQuery(query, schema.Id, status);

                var contentCount = Collection.Find(filter).CountDocumentsAsync();
                var contentItems =
                    Collection.Find(filter)
                        .ContentTake(query)
                        .ContentSkip(query)
                        .ContentSort(query)
                        .Not(x => x.DataText)
                        .ToListAsync();

                await Task.WhenAll(contentItems, contentCount);

                foreach (var entity in contentItems.Result)
                {
                    entity.ParseData(schema.SchemaDef);
                }

                return ResultList.Create<IContentEntity>(contentCount.Result, contentItems.Result);
            }
            catch (MongoQueryException ex)
            {
                if (ex.Message.Contains("17406"))
                {
                    throw new DomainException("Result set is too large to be retrieved. Use $top parameter to reduce the number of items.");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, HashSet<Guid> ids, Status[] status = null)
        {
            var find =
                status != null && status.Length > 0 ?
                    Collection.Find(x => x.IndexedSchemaId == schema.Id && ids.Contains(x.Id) && x.IsDeleted != true && status.Contains(x.Status)) :
                    Collection.Find(x => x.IndexedSchemaId == schema.Id && ids.Contains(x.Id));

            var contentItems = find.Not(x => x.DataText).ToListAsync();
            var contentCount = find.CountDocumentsAsync();

            await Task.WhenAll(contentItems, contentCount);

            foreach (var entity in contentItems.Result)
            {
                entity.ParseData(schema.SchemaDef);
            }

            return ResultList.Create<IContentEntity>(contentCount.Result, contentItems.Result);
        }

        public Task CleanupAsync(Guid id)
        {
            return Collection.UpdateManyAsync(
                Filter.And(
                    Filter.AnyEq(x => x.ReferencedIds, id),
                    Filter.AnyNe(x => x.ReferencedIdsDeleted, id)),
                Update.AddToSet(x => x.ReferencedIdsDeleted, id));
        }

        public Task RemoveAsync(Guid id)
        {
            return Collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
