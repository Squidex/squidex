﻿// ==========================================================================
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
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryIdsAsync : OperationBase
    {
        private static readonly List<(Guid SchemaId, Guid Id)> EmptyIds = new List<(Guid SchemaId, Guid Id)>();
        private readonly IAppProvider appProvider;

        public QueryIdsAsync(IAppProvider appProvider)
        {
            this.appProvider = appProvider;
        }

        protected override Task PrepareAsync(CancellationToken ct = default)
        {
            var index1 =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.Id)
                    .Ascending(x => x.ReferencedIds));

            var index2 =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedSchemaId));

            return Collection.Indexes.CreateManyAsync(new[] { index1, index2, }, ct);
        }

        public async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> DoAsync(Guid appId, HashSet<Guid> ids)
        {
            var contentEntities =
                await Collection.Find(Filter.And(Filter.Eq(x => x.IndexedAppId, appId), Filter.In(x => x.Id, ids))).Only(x => x.Id, x => x.IndexedSchemaId)
                    .ToListAsync();

            return contentEntities.Select(x => (Guid.Parse(x["_si"].AsString), Guid.Parse(x["_id"].AsString))).ToList();
        }

        public async Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> DoAsync(Guid appId, Guid schemaId, FilterNode<ClrValue> filterNode)
        {
            var schema = await appProvider.GetSchemaAsync(appId, schemaId);

            if (schema == null)
            {
                return EmptyIds;
            }

            var filter = BuildFilter(filterNode.AdjustToModel(schema.SchemaDef, true), schemaId);

            var contentEntities =
                await Collection.Find(filter).Only(x => x.Id, x => x.IndexedSchemaId)
                    .ToListAsync();

            return contentEntities.Select(x => (Guid.Parse(x["_si"].AsString), Guid.Parse(x["_id"].AsString))).ToList();
        }

        public static FilterDefinition<MongoContentEntity> BuildFilter(FilterNode<ClrValue>? filterNode, Guid schemaId)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedSchemaId, schemaId),
                Filter.Ne(x => x.IsDeleted, true),
            };

            if (filterNode != null)
            {
                filters.Add(filterNode.BuildFilter<MongoContentEntity>());
            }

            return Filter.And(filters);
        }
    }
}
