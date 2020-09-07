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
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryIdsAsync : OperationBase
    {
        private static readonly IReadOnlyList<(DomainId SchemaId, DomainId Id)> EmptyIds = new List<(DomainId SchemaId, DomainId Id)>();
        private static readonly Lazy<string> IdField = new Lazy<string>(GetIdField);
        private static readonly Lazy<string> SchemaIdField = new Lazy<string>(GetSchemaIdField);
        private readonly IAppProvider appProvider;

        public QueryIdsAsync(IAppProvider appProvider)
        {
            this.appProvider = appProvider;
        }

        protected override Task PrepareAsync(CancellationToken ct = default)
        {
            var index =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.IndexedSchemaId)
                    .Ascending(x => x.IsDeleted));

            return Collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id)>> DoAsync(DomainId appId, HashSet<DomainId> ids)
        {
            var documentIds = ids.Select(x => DomainId.Combine(appId, x));

            var filter =
                Filter.And(
                    Filter.In(x => x.DocumentId, documentIds),
                    Filter.Ne(x => x.IsDeleted, true));

            var contentEntities =
                await Collection.Find(filter).Only(x => x.Id, x => x.IndexedSchemaId)
                    .ToListAsync();

            return contentEntities.Select(x => (DomainId.Create(x[SchemaIdField.Value].AsString), DomainId.Create(x[IdField.Value].AsString))).ToList();
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id)>> DoAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode)
        {
            var schema = await appProvider.GetSchemaAsync(appId, schemaId, false);

            if (schema == null)
            {
                return EmptyIds;
            }

            var filter = BuildFilter(filterNode.AdjustToModel(schema.SchemaDef), appId, schemaId);

            var contentEntities =
                await Collection.Find(filter).Only(x => x.Id, x => x.IndexedSchemaId)
                    .ToListAsync();

            return contentEntities.Select(x => (DomainId.Create(x[SchemaIdField.Value].AsString), DomainId.Create(x[IdField.Value].AsString))).ToList();
        }

        public static FilterDefinition<MongoContentEntity> BuildFilter(FilterNode<ClrValue>? filterNode, DomainId appId, DomainId schemaId)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.Eq(x => x.IndexedSchemaId, schemaId),
                Filter.Ne(x => x.IsDeleted, true)
            };

            if (filterNode != null)
            {
                filters.Add(filterNode.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }

        private static string GetIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).GetMemberMap(nameof(MongoContentEntity.Id)).ElementName;
        }

        private static string GetSchemaIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).GetMemberMap(nameof(MongoContentEntity.IndexedSchemaId)).ElementName;
        }
    }
}