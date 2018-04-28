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
using MongoDB.Driver;
using NodaTime;
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

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider)
            : base(database)
        {
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;
        }

        protected override string CollectionName()
        {
            return "States_Contents";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection)
        {
            await collection.Indexes.TryDropOneAsync("si_1_st_1_dl_1_dt_text");

            await collection.Indexes.CreateOneAsync(
                Index
                    .Text(x => x.DataText)
                    .Ascending(x => x.SchemaIdId)
                    .Ascending(x => x.Status)
                    .Ascending(x => x.IsDeleted));

            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.SchemaIdId)
                    .Ascending(x => x.Id)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.Status));

            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.ReferencedIds));
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery)
        {
            try
            {
                var propertyCalculator = FindExtensions.CreatePropertyCalculator(schema.SchemaDef);

                var filter = FindExtensions.BuildQuery(odataQuery, schema.Id, status, propertyCalculator);

                var contentCount = Collection.Find(filter).CountAsync();
                var contentItems =
                    Collection.Find(filter)
                        .ContentTake(odataQuery)
                        .ContentSkip(odataQuery)
                        .ContentSort(odataQuery, propertyCalculator)
                        .ToListAsync();

                await Task.WhenAll(contentItems, contentCount);

                foreach (var entity in contentItems.Result)
                {
                    entity.ParseData(schema.SchemaDef);
                }

                return ResultList.Create<IContentEntity>(contentItems.Result, contentCount.Result);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported.");
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

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            var find = Collection.Find(x => x.SchemaIdId == schema.Id && ids.Contains(x.Id) && x.IsDeleted == false && status.Contains(x.Status));

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
                await Collection.Find(x => x.SchemaIdId == schemaId && ids.Contains(x.Id) && x.IsDeleted == false).Only(x => x.Id)
                    .ToListAsync();

            return ids.Except(contentEntities.Select(x => Guid.Parse(x["id"].AsString))).ToList();
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id)
        {
            var contentEntity =
                await Collection.Find(x => x.SchemaIdId == schema.Id && x.Id == id && x.IsDeleted == false)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }

        public Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            return Collection.Find(x => x.ScheduledAt < now && x.IsDeleted == false)
                .ForEachAsync(c =>
                {
                    callback(c);
                });
        }

        public Task DeleteArchiveAsync()
        {
            return Database.DropCollectionAsync("States_Contents_Archive");
        }
    }
}
