// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using NJsonSchema;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Queries.OData;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentQueryParser
    {
        private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(60);
        private readonly EdmModel genericEdmModel = BuildEdmModel("Generic", "Content", new EdmModel(), null);
        private readonly JsonSchema genericJsonSchema = ContentJsonSchemaBuilder.BuildSchema(null, false, true);
        private readonly IMemoryCache cache;
        private readonly IJsonSerializer jsonSerializer;
        private readonly IAppProvider appprovider;
        private readonly ITextIndex textIndex;
        private readonly ContentOptions options;

        public ContentQueryParser(IAppProvider appprovider, ITextIndex textIndex, IOptions<ContentOptions> options,
            IMemoryCache cache, IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
            this.appprovider = appprovider;
            this.textIndex = textIndex;
            this.cache = cache;
            this.options = options.Value;
        }

        public virtual async Task<Q> ParseAsync(Context context, Q q, ISchemaEntity? schema = null)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(q, nameof(q));

            using (Telemetry.Activities.StartActivity("ContentQueryParser/ParseAsync"))
            {
                var query = await ParseClrQueryAsync(context, q, schema);

                await TransformFilterAsync(query, context, schema);

                WithSorting(query);
                WithPaging(query, q);

                q = q.WithQuery(query);

                if (context.ShouldSkipTotal())
                {
                    q = q.WithoutTotal();
                }

                return q;
            }
        }

        private async Task TransformFilterAsync(ClrQuery query, Context context, ISchemaEntity? schema)
        {
            if (query.Filter != null && schema != null)
            {
                query.Filter = await GeoQueryTransformer.TransformAsync(query.Filter, context, schema, textIndex);
            }

            if (!string.IsNullOrWhiteSpace(query.FullText))
            {
                if (schema == null)
                {
                    throw new InvalidOperationException();
                }

                var textQuery = new TextQuery(query.FullText, TextFilter.ShouldHaveSchemas(schema.Id));

                var fullTextIds = await textIndex.SearchAsync(context.App, textQuery, context.Scope());
                var fullTextFilter = ClrFilter.Eq("id", "__notfound__");

                if (fullTextIds?.Any() == true)
                {
                    fullTextFilter = ClrFilter.In("id", fullTextIds.Select(x => x.ToString()).ToList());
                }

                if (query.Filter != null)
                {
                    query.Filter = ClrFilter.And(query.Filter, fullTextFilter);
                }
                else
                {
                    query.Filter = fullTextFilter;
                }

                query.FullText = null;
            }
        }

        private async Task<ClrQuery> ParseClrQueryAsync(Context context, Q q, ISchemaEntity? schema)
        {
            var components = ResolvedComponents.Empty;

            if (schema != null)
            {
                components = await appprovider.GetComponentsAsync(schema);
            }

            var query = q.Query;

            if (!string.IsNullOrWhiteSpace(q.QueryAsJson))
            {
                query = ParseJson(context, schema, q.QueryAsJson, components);
            }
            else if (q?.JsonQuery != null)
            {
                query = ParseJson(context, schema, q.JsonQuery, components);
            }
            else if (!string.IsNullOrWhiteSpace(q?.QueryAsOdata))
            {
                query = ParseOData(context, schema, q.QueryAsOdata, components);
            }

            return query;
        }

        private static void WithSorting(ClrQuery query)
        {
            if (query.Sort.Count == 0)
            {
                query.Sort.Add(new SortNode(new List<string> { "lastModified" }, SortOrder.Descending));
            }

            if (!query.Sort.Any(x => string.Equals(x.Path.ToString(), "id", StringComparison.OrdinalIgnoreCase)))
            {
                query.Sort.Add(new SortNode(new List<string> { "id" }, SortOrder.Ascending));
            }
        }

        private void WithPaging(ClrQuery query, Q q)
        {
            if (query.Take <= 0 || query.Take == long.MaxValue)
            {
                if (q.Ids != null && q.Ids.Count > 0)
                {
                    query.Take = q.Ids.Count;
                }
                else
                {
                    query.Take = options.DefaultPageSize;
                }
            }
            else if (query.Take > options.MaxResults)
            {
                query.Take = options.MaxResults;
            }
        }

        private ClrQuery ParseJson(Context context, ISchemaEntity? schema, Query<IJsonValue> query,
            ResolvedComponents components)
        {
            var jsonSchema = BuildJsonSchema(context, schema, components);

            return jsonSchema.Convert(query);
        }

        private ClrQuery ParseJson(Context context, ISchemaEntity? schema, string json,
            ResolvedComponents components)
        {
            var jsonSchema = BuildJsonSchema(context, schema, components);

            return jsonSchema.Parse(json, jsonSerializer);
        }

        private ClrQuery ParseOData(Context context, ISchemaEntity? schema, string odata,
            ResolvedComponents components)
        {
            try
            {
                var model = BuildEdmModel(context, schema, components);

                return model.ParseQuery(odata).ToQuery();
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw new ValidationException(T.Get("common.odataNotSupported", new { odata }));
            }
            catch (ODataException ex)
            {
                var message = ex.Message;

                throw new ValidationException(T.Get("common.odataFailure", new { odata, message }), ex);
            }
            catch (Exception)
            {
                throw new ValidationException(T.Get("common.odataNotSupported", new { odata }));
            }
        }

        private JsonSchema BuildJsonSchema(Context context, ISchemaEntity? schema,
            ResolvedComponents components)
        {
            if (schema == null)
            {
                return genericJsonSchema;
            }

            var cacheKey = BuildJsonCacheKey(context.App, schema, context.IsFrontendClient);

            var result = cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTime;

                return BuildJsonSchema(schema.SchemaDef, context.App, components, context.IsFrontendClient);
            });

            return result;
        }

        private static JsonSchema BuildJsonSchema(Schema schema, IAppEntity app,
            ResolvedComponents components, bool withHiddenFields)
        {
            var dataSchema = schema.BuildJsonSchema(app.PartitionResolver(), components, withHiddenFields);

            return ContentJsonSchemaBuilder.BuildSchema(dataSchema, false, true);
        }

        private IEdmModel BuildEdmModel(Context context, ISchemaEntity? schema,
            ResolvedComponents components)
        {
            if (schema == null)
            {
                return genericEdmModel;
            }

            var cacheKey = BuildEmdCacheKey(context.App, schema, context.IsFrontendClient);

            var result = cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTime;

                return BuildEdmModel(schema.SchemaDef, context.App, components, context.IsFrontendClient);
            });

            return result;
        }

        private static EdmModel BuildEdmModel(Schema schema, IAppEntity app,
            ResolvedComponents components, bool withHiddenFields)
        {
            var model = new EdmModel();

            var pascalAppName = app.Name.ToPascalCase();
            var pascalSchemaName = schema.Name.ToPascalCase();

            var typeFactory = new EdmTypeFactory(name =>
            {
                var finalName = pascalSchemaName;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    finalName += ".";
                    finalName += name;
                }

                var result = model.SchemaElements.OfType<EdmComplexType>().FirstOrDefault(x => x.Name == finalName);

                if (result != null)
                {
                    return (result, false);
                }

                result = new EdmComplexType(pascalAppName, finalName);

                model.AddElement(result);

                return (result, true);
            });

            var schemaType = schema.BuildEdmType(withHiddenFields, app.PartitionResolver(), typeFactory, components);

            return BuildEdmModel(app.Name.ToPascalCase(), schema.Name, model, schemaType);
        }

        private static EdmModel BuildEdmModel(string modelName, string name, EdmModel model, EdmComplexType? schemaType)
        {
            var entityType = new EdmEntityType(modelName, name);

            entityType.AddStructuralProperty("id", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("isDeleted", EdmPrimitiveTypeKind.Boolean);
            entityType.AddStructuralProperty("created", EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty("createdBy", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("lastModified", EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty("lastModifiedBy", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("newStatus", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("status", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("version", EdmPrimitiveTypeKind.Int32);

            if (schemaType != null)
            {
                entityType.AddStructuralProperty("data", new EdmComplexTypeReference(schemaType, false));

                model.AddElement(schemaType);
            }

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("ContentSet", entityType);

            model.AddElement(container);
            model.AddElement(entityType);

            return model;
        }

        private static string BuildEmdCacheKey(IAppEntity app, ISchemaEntity schema, bool withHidden)
        {
            return $"EDM/{app.Version}/{schema.Id}_{schema.Version}/{withHidden}";
        }

        private static string BuildJsonCacheKey(IAppEntity app, ISchemaEntity schema, bool withHidden)
        {
            return $"JSON/{app.Version}/{schema.Id}_{schema.Version}/{withHidden}";
        }
    }
}
