// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryIdsAsync : OperationBase
    {
        private static readonly List<(DomainId SchemaId, DomainId Id, Status Status)> EmptyIds = new List<(DomainId SchemaId, DomainId Id, Status Status)>();
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

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> DoAsync(DomainId appId, HashSet<DomainId> ids)
        {
            var documentIds = ids.Select(x => DomainId.Combine(appId, x));

            var filter =
                Filter.And(
                    Filter.In(x => x.DocumentId, documentIds),
                    Filter.Ne(x => x.IsDeleted, true));

            return await SearchAsync(filter);
        }

        public async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> DoAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode)
        {
            var schema = await appProvider.GetSchemaAsync(appId, schemaId, false);

            if (schema == null)
            {
                return EmptyIds;
            }

            var filter = BuildFilter(filterNode.AdjustToModel(schema.SchemaDef), appId, schemaId);

            return await SearchAsync(filter);
        }

        private async Task<IReadOnlyList<(DomainId SchemaId, DomainId Id, Status Status)>> SearchAsync(FilterDefinition<MongoContentEntity> filter)
        {
            var contentEntities =
                await Collection.Find(filter).Only(x => x.Id, x => x.IndexedSchemaId, x => x.Status)
                    .ToListAsync();

            return contentEntities.Select(x => (
                DomainId.Create(x[Fields.SchemaId].AsString),
                DomainId.Create(x[Fields.Id].AsString),
                new Status(x[Fields.Status].AsString)
            )).ToList();
        }

        private static FilterDefinition<MongoContentEntity> BuildFilter(FilterNode<ClrValue>? filterNode, DomainId appId, DomainId schemaId)
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
    }
}