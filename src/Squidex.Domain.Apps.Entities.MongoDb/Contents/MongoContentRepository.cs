// ==========================================================================
//  MongoContentRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.UriParser;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : MongoRepositoryBase<MongoContentEntity>,
        IEventConsumer,
        IContentRepository,
        ISnapshotStore<ContentState, Guid>
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
            return "Snapshots_Contents";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoContentEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.Id)
                    .Ascending(x => x.Version));

            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.Id)
                    .Descending(x => x.Version));

            await collection.Indexes.CreateOneAsync(
                Index
                    .Ascending(x => x.SchemaId)
                    .Descending(x => x.IsLatest)
                    .Descending(x => x.LastModified));

            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.ReferencedIds));
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Status));
            await collection.Indexes.CreateOneAsync(Index.Text(x => x.DataText));
        }

        public async Task WriteAsync(Guid key, ContentState value, long oldVersion, long newVersion)
        {
            var documentId = $"{key}_{newVersion}";

            var schema = await appProvider.GetSchemaAsync(value.AppId, value.SchemaId);

            if (schema == null)
            {
                throw new InvalidOperationException($"Cannot find schema {value.SchemaId}");
            }

            var idData = value.Data?.ToIdModel(schema.SchemaDef, true);

            var document = SimpleMapper.Map(value, new MongoContentEntity
            {
                DocumentId = documentId,
                DataText = idData?.ToFullText(),
                DataByIds = idData,
                IsLatest = true,
                ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef),
            });

            try
            {
                await Collection.InsertOneAsync(document);

                await Collection.UpdateManyAsync(x => x.Id == value.Id && x.Version < value.Version, Update.Set(x => x.IsLatest, false));
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    var existingVersion =
                        await Collection.Find(x => x.Id == value.Id && x.IsLatest).Only(x => x.Id, x => x.Version)
                            .FirstOrDefaultAsync();

                    if (existingVersion != null)
                    {
                        throw new InconsistentStateException(existingVersion["Version"].AsInt64, oldVersion, ex);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<(ContentState Value, long Version)> ReadAsync(Guid key)
        {
            var contentEntity =
                await Collection.Find(x => x.Id == key && x.IsLatest)
                    .FirstOrDefaultAsync();

            if (contentEntity != null)
            {
                var schema = await appProvider.GetSchemaAsync(contentEntity.AppId, contentEntity.SchemaId);

                if (schema == null)
                {
                    throw new InvalidOperationException($"Cannot find schema {contentEntity.SchemaId}");
                }

                contentEntity?.ParseData(schema.SchemaDef);

                return (SimpleMapper.Map(contentEntity, new ContentState()), contentEntity.Version);
            }

            return (null, EtagVersion.NotFound);
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery)
        {
            IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
            try
            {
                cursor =
                    Collection
                        .Find(odataQuery, schema.Id, schema.SchemaDef, status)
                        .Take(odataQuery)
                        .Skip(odataQuery)
                        .Sort(odataQuery, schema.SchemaDef);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }

            var contentEntities = await cursor.ToListAsync();

            foreach (var entity in contentEntities)
            {
                entity.ParseData(schema.SchemaDef);
            }

            return contentEntities;
        }

        public async Task<long> CountAsync(IAppEntity app, ISchemaEntity schema, Status[] status, ODataUriParser odataQuery)
        {
            IFindFluent<MongoContentEntity, MongoContentEntity> cursor;
            try
            {
                cursor = Collection.Find(odataQuery, schema.Id, schema.SchemaDef, status);
            }
            catch (NotSupportedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }
            catch (NotImplementedException)
            {
                throw new ValidationException("This odata operation is not supported.");
            }

            return await cursor.CountAsync();
        }

        public async Task<long> CountAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            var contentsCount =
                await Collection.Find(x => ids.Contains(x.Id) && x.IsLatest)
                    .CountAsync();

            return contentsCount;
        }

        public async Task<IReadOnlyList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            var contentEntities =
                await Collection.Find(x => ids.Contains(x.Id) && x.IsLatest)
                    .ToListAsync();

            foreach (var entity in contentEntities)
            {
                entity.ParseData(schema.SchemaDef);
            }

            return contentEntities.OfType<IContentEntity>().ToList();
        }

        public async Task<IReadOnlyList<Guid>> QueryNotFoundAsync(Guid appId, Guid schemaId, IList<Guid> contentIds)
        {
            var contentEntities =
                await Collection.Find(x => contentIds.Contains(x.Id) && x.AppId == appId).Only(x => x.Id)
                    .ToListAsync();

            return contentIds.Except(contentEntities.Select(x => Guid.Parse(x["_id"].AsString))).ToList();
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id, long version)
        {
            var contentEntity =
                await Collection.Find(x => x.Id == id && x.Version >= version).SortBy(x => x.Version)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Guid id)
        {
            var contentEntity =
                await Collection.Find(x => x.Id == id && x.IsLatest)
                    .FirstOrDefaultAsync();

            contentEntity?.ParseData(schema.SchemaDef);

            return contentEntity;
        }
    }
}
