// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryInDedicatedCollection : MongoBase<MongoContentEntity>
    {
        private readonly ConcurrentDictionary<(DomainId, DomainId), Task<IMongoCollection<MongoContentEntity>>> collections =
            new ConcurrentDictionary<(DomainId, DomainId), Task<IMongoCollection<MongoContentEntity>>>();

        private readonly IMongoClient mongoClient;
        private readonly string mongoPrefix;

        [BsonIgnoreExtraElements]
        internal sealed class IdOnly
        {
            [BsonId]
            [BsonElement("_id")]
            public DomainId Id { get; set; }

            public MongoContentEntity[] Joined { get; set; }
        }

        public QueryInDedicatedCollection(IMongoClient mongoClient, string mongoPrefix)
        {
            this.mongoClient = mongoClient;
            this.mongoPrefix = mongoPrefix;
        }

        private Task<IMongoCollection<MongoContentEntity>> GetCollectionAsync(DomainId appId, DomainId schemaId)
        {
#pragma warning disable MA0106 // Avoid closure by using an overload with the 'factoryArgument' parameter
            return collections.GetOrAdd((appId, schemaId), async key =>
            {
                var (appId, schemaId) = key;

                var schemaDatabase = mongoClient.GetDatabase($"{mongoPrefix}_{appId}");
                var schemaCollection = schemaDatabase.GetCollection<MongoContentEntity>($"{schemaId}");

                await schemaCollection.Indexes.CreateManyAsync(
                    new[]
                    {
                        new CreateIndexModel<MongoContentEntity>(Index
                            .Descending(x => x.LastModified)
                            .Ascending(x => x.Id)
                            .Ascending(x => x.IsDeleted)
                            .Ascending(x => x.ReferencedIds)),
                        new CreateIndexModel<MongoContentEntity>(Index
                            .Ascending(x => x.IndexedSchemaId)
                            .Ascending(x => x.IsDeleted)
                            .Descending(x => x.LastModified))
                    });

                return schemaCollection;
            });
#pragma warning restore MA0106 // Avoid closure by using an overload with the 'factoryArgument' parameter
        }

        public async Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(IAppEntity app, ISchemaEntity schema, FilterNode<ClrValue> filterNode,
            CancellationToken ct)
        {
            var filter = BuildFilter(filterNode.AdjustToModel(app.Id));

            var contentCollection = await GetCollectionAsync(schema.AppId.Id, schema.Id);
            var contentItems = await contentCollection.FindStatusAsync(filter, ct);

            return contentItems.Select(x => new ContentIdStatus(x.IndexedSchemaId, x.Id, x.Status)).ToList();
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(ISchemaEntity schema, Q q,
            CancellationToken ct)
        {
            var query = q.Query.AdjustToModel(schema.AppId.Id);

            var filter = CreateFilter(query, q.Reference, q.CreatedBy);

            var contentCollection = await GetCollectionAsync(schema.AppId.Id, schema.Id);
            var contentEntities = await FindContentsAsync(contentCollection, query, filter, ct);
            var contentTotal = (long)contentEntities.Count;

            if (contentTotal >= q.Query.Take || q.Query.Skip > 0)
            {
                if (q.NoTotal || (q.NoSlowTotal && q.Query.Filter != null))
                {
                    contentTotal = -1;
                }
                else
                {
                    contentTotal = await contentCollection.Find(filter).CountDocumentsAsync(ct);
                }
            }

            return ResultList.Create<IContentEntity>(contentTotal, contentEntities);
        }

        private static async Task<List<MongoContentEntity>> FindContentsAsync(IMongoCollection<MongoContentEntity> collection, ClrQuery query, FilterDefinition<MongoContentEntity> filter,
            CancellationToken ct)
        {
            var result =
                collection.Find(filter)
                    .QuerySort(query)
                    .QueryLimit(query)
                    .QuerySkip(query)
                    .ToListAsync(ct);

            return await result;
        }

        public async Task UpsertVersionedAsync(DomainId documentId, long oldVersion, MongoContentEntity value,
            CancellationToken ct = default)
        {
            var collection = await GetCollectionAsync(value.AppId.Id, value.SchemaId.Id);

            await collection.UpsertVersionedAsync(documentId, oldVersion, value.Version, value, ct);
        }

        public async Task RemoveAsync(MongoContentEntity value,
            CancellationToken ct = default)
        {
            var collection = await GetCollectionAsync(value.AppId.Id, value.SchemaId.Id);

            await collection.DeleteOneAsync(x => x.DocumentId == value.DocumentId, null, ct);
        }

        private static FilterDefinition<MongoContentEntity> BuildFilter(FilterNode<ClrValue>? filter)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Exists(x => x.LastModified),
                Filter.Exists(x => x.Id)
            };

            if (filter?.HasField("dl") != true)
            {
                filters.Add(Filter.Ne(x => x.IsDeleted, true));
            }

            if (filter != null)
            {
                filters.Add(filter.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(ClrQuery? query,
            DomainId referenced, RefToken? createdBy)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Gt(x => x.LastModified, default),
                Filter.Gt(x => x.Id, default)
            };

            if (query?.HasFilterField("dl") != true)
            {
                filters.Add(Filter.Ne(x => x.IsDeleted, true));
            }

            if (query?.Filter != null)
            {
                filters.Add(query.Filter.BuildFilter<MongoContentEntity>());
            }

            if (referenced != default)
            {
                filters.Add(Filter.AnyEq(x => x.ReferencedIds, referenced));
            }

            if (createdBy != null)
            {
                filters.Add(Filter.Eq(x => x.CreatedBy, createdBy));
            }

            return Filter.And(filters);
        }
    }
}
