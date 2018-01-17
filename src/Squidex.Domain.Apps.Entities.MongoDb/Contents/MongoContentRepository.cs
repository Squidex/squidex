// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.UriParser;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : MongoRepositoryBase<MongoContentEntity>, IContentRepository
    {
        private readonly IAppProvider appProvider;
        private readonly IMongoCollection<MongoContentEntity> archiveCollection;

        protected IMongoCollection<MongoContentEntity> ArchiveCollection
        {
            get { return archiveCollection; }
        }

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider)
            : base(database)
        {
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;

            archiveCollection = database.GetCollection<MongoContentEntity>("States_Contents_Archive");
        }

        protected override string CollectionName()
        {
            return "States_Contents";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection)
        {
            await archiveCollection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.Id)
                    .Ascending(x => x.Version));

            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.SchemaId)
                    .Ascending(x => x.Status)
                    .Ascending(x => x.IsDeleted)
                    .Text(x => x.DataText));

            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.SchemaId)
                    .Ascending(x => x.Id)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.Status));

            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.ReferencedIds));
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery)
        {
            FilterDefinition<MongoContentEntity> filter;
            try
            {
                filter = FindExtensions.BuildQuery(odataQuery, schema.Id, schema.SchemaDef, status);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }

            var contentItems = Collection.Find(filter).Take(odataQuery).Skip(odataQuery).Sort(odataQuery, schema.SchemaDef).ToListAsync();
            var contentCount = Collection.Find(filter).CountAsync();

            await Task.WhenAll(contentItems, contentCount);

            foreach (var entity in contentItems.Result)
            {
                entity.ParseData(schema.SchemaDef);
            }

            return ResultList.Create<IContentEntity>(contentItems.Result, contentCount.Result);
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            var find = Collection.Find(x => x.SchemaId == schema.Id && ids.Contains(x.Id) && !x.IsDeleted && status.Contains(x.Status));

            var contentItems = find.ToListAsync();
            var contentCount = find.CountAsync();

            await Task.WhenAll(contentItems, contentCount);

            foreach (var entity in contentItems.Result)
            {
                entity.ParseData(schema.SchemaDef);
            }

            return ResultList.Create<IContentEntity>(contentItems.Result, contentCount.Result);
        }

        public async Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> ids)
        {
            var contentEntities =
                await Collection.Find(x => x.SchemaId == schemaId && ids.Contains(x.Id) && !x.IsDeleted).Only(x => x.Id)
                    .ToListAsync();

            return ids.Except(contentEntities.Select(x => Guid.Parse(x["id"].AsString))).ToList();
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id, long version)
        {
            var contentEntity =
                await ArchiveCollection.Find(x => x.Id == id && x.Version >= version).SortBy(x => x.Version)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id)
        {
            var contentEntity =
                await Collection.Find(x => x.SchemaId == schema.Id && x.Id == id && !x.IsDeleted)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }

        public override async Task ClearAsync()
        {
            await Database.DropCollectionAsync("States_Contents_Archive");

            await base.ClearAsync();
        }
    }
}
