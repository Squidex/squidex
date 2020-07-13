// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    internal sealed class QueryContentsByIds : OperationBase
    {
        private readonly DataConverter converter;
        private readonly IAppProvider appProvider;

        public QueryContentsByIds(DataConverter converter, IAppProvider appProvider)
        {
            this.converter = converter;

            this.appProvider = appProvider;
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> DoAsync(DomainId appId, ISchemaEntity? schema, HashSet<DomainId> ids, bool canCache)
        {
            Guard.NotNull(ids, nameof(ids));

            var find = Collection.Find(CreateFilter(appId, ids));

            var contentItems = await find.ToListAsync();
            var contentSchemas = await GetSchemasAsync(appId, schema, contentItems, canCache);

            var result = new List<(IContentEntity Content, ISchemaEntity Schema)>();

            foreach (var contentEntity in contentItems)
            {
                if (contentSchemas.TryGetValue(contentEntity.IndexedSchemaId, out var contentSchema))
                {
                    contentEntity.ParseData(contentSchema.SchemaDef, converter);

                    result.Add((contentEntity, contentSchema));
                }
            }

            return result;
        }

        private async Task<IDictionary<DomainId, ISchemaEntity>> GetSchemasAsync(DomainId appId, ISchemaEntity? schema, List<MongoContentEntity> contentItems, bool canCache)
        {
            var schemas = new Dictionary<DomainId, ISchemaEntity>();

            if (schema != null)
            {
                schemas[schema.Id] = schema;
            }

            var schemaIds = contentItems.Select(x => x.IndexedSchemaId).Distinct();

            foreach (var schemaId in schemaIds)
            {
                if (!schemas.ContainsKey(schemaId))
                {
                    var found = await appProvider.GetSchemaAsync(appId, schemaId, false, canCache);

                    if (found != null)
                    {
                        schemas[schemaId] = found;
                    }
                }
            }

            return schemas;
        }

        private static FilterDefinition<MongoContentEntity> CreateFilter(DomainId appId, ICollection<DomainId> ids)
        {
            var filters = new List<FilterDefinition<MongoContentEntity>>
            {
                Filter.Ne(x => x.IsDeleted, true)
            };

            if (ids != null && ids.Count > 0)
            {
                var documentIds = ids.Select(x => DomainId.Combine(appId, x)).ToList();

                if (ids.Count > 1)
                {
                    filters.Add(
                        Filter.Or(
                            Filter.In(x => x.DocumentId, documentIds)));
                }
                else
                {
                    var first = documentIds.First();

                    filters.Add(
                        Filter.Or(
                            Filter.Eq(x => x.DocumentId, first)));
                }
            }

            return Filter.And(filters);
        }
    }
}
