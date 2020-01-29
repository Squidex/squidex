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
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryContentsByIds : OperationBase
    {
        private readonly IJsonSerializer serializer;
        private readonly IAppProvider appProvider;

        public QueryContentsByIds(IMongoCollection<MongoContentEntity> collection, IJsonSerializer serializer, IAppProvider appProvider)
            : base(collection)
        {
            this.serializer = serializer;

            this.appProvider = appProvider;
        }

        public override Task PrepareAsync(CancellationToken ct = default)
        {
            var index =
                new CreateIndexModel<MongoContentEntity>(Index
                    .Ascending(x => x.IndexedAppId)
                    .Ascending(x => x.IsDeleted)
                    .Ascending(x => x.Status)
                    .Descending(x => x.LastModified));

            return Collection.Indexes.CreateOneAsync(index, cancellationToken: ct);
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> DoAsync(Guid appId, ISchemaEntity? schema, HashSet<Guid> ids, Status[]? status, bool includeDraft)
        {
            Guard.NotNull(ids);
            Guard.NotNull(schema);

            var find = Collection.Find(CreateFilter(appId, ids, status)).WithoutDraft(includeDraft);

            var contentItems = await find.ToListAsync();
            var contentSchemas = await GetSchemasAsync(appId, schema, contentItems);

            var result = new List<(IContentEntity Content, ISchemaEntity Schema)>();

            foreach (var contentEntity in contentItems)
            {
                if (contentEntity.HasStatus(status) && contentSchemas.TryGetValue(contentEntity.IndexedSchemaId, out var contentSchema))
                {
                    contentEntity.ParseData(contentSchema.SchemaDef, serializer);

                    result.Add((contentEntity, contentSchema));
                }
            }

            return result;
        }

        private async Task<IDictionary<Guid, ISchemaEntity>> GetSchemasAsync(Guid appId, ISchemaEntity? schema, List<MongoContentEntity> contentItems)
        {
            var schemas = new Dictionary<Guid, ISchemaEntity>();

            if (schema != null)
            {
                schemas[schema.Id] = schema;
            }

            var misingSchemaIds = contentItems.Select(x => x.IndexedSchemaId).Distinct().Where(x => !schemas.ContainsKey(x));
            var missingSchemas = await Task.WhenAll(misingSchemaIds.Select(x => appProvider.GetSchemaAsync(appId, x)));

            foreach (var missingSchema in missingSchemas)
            {
                schemas[missingSchema.Id] = missingSchema;
            }

            return schemas;
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(Guid appId, ICollection<Guid> ids, Status[]? status)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Eq(x => x.IndexedAppId, appId),
                Filter.Ne(x => x.IsDeleted, true)
            };

            if (status != null)
            {
                filters.Add(Filter.In(x => x.Status, status));
            }
            else
            {
                filters.Add(Filter.Exists(x => x.Status));
            }

            if (ids != null && ids.Count > 0)
            {
                if (ids.Count > 1)
                {
                    filters.Add(
                        Filter.Or(
                            Filter.In(x => x.Id, ids)));
                }
                else
                {
                    var first = ids.First();

                    filters.Add(
                        Filter.Or(
                            Filter.Eq(x => x.Id, first)));
                }
            }

            return Filter.And(filters);
        }
    }
}
