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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Queries.OData;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentQueryParser
    {
        private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(60);
        private readonly EdmModel genericEdmModel = BuildEdmModel("Generic", "Content", new EdmModel(), null);
        private readonly JsonSchema genericJsonSchema = BuildJsonSchema("Content", null);
        private readonly IMemoryCache cache;
        private readonly IJsonSerializer jsonSerializer;
        private readonly ContentOptions options;

        public ContentQueryParser(IMemoryCache cache, IJsonSerializer jsonSerializer, IOptions<ContentOptions> options)
        {
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(options, nameof(options));

            this.jsonSerializer = jsonSerializer;
            this.cache = cache;
            this.options = options.Value;
        }

        public virtual ValueTask<Q> ParseAsync(Context context, Q q, ISchemaEntity? schema = null)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(q, nameof(q));

            using (Profiler.TraceMethod<ContentQueryParser>())
            {
                var query = q.Query;

                if (!string.IsNullOrWhiteSpace(q.JsonQueryString))
                {
                    query = ParseJson(context, schema, q.JsonQueryString);
                }
                else if (q?.JsonQuery != null)
                {
                    query = ParseJson(context, schema, q.JsonQuery);
                }
                else if (!string.IsNullOrWhiteSpace(q?.ODataQuery))
                {
                    query = ParseOData(context, schema, q.ODataQuery);
                }

                if (query.Sort.Count == 0)
                {
                    query.Sort.Add(new SortNode(new List<string> { "lastModified" }, SortOrder.Descending));
                }

                if (!query.Sort.Any(x => string.Equals(x.Path.ToString(), "id", StringComparison.OrdinalIgnoreCase)))
                {
                    query.Sort.Add(new SortNode(new List<string> { "id" }, SortOrder.Ascending));
                }

                if (query.Take == long.MaxValue)
                {
                    query.Take = options.DefaultPageSize;
                }
                else if (query.Take > options.MaxResults)
                {
                    query.Take = options.MaxResults;
                }

                q = q!.WithQuery(query);

                return new ValueTask<Q>(q);
            }
        }

        private ClrQuery ParseJson(Context context, ISchemaEntity? schema, Query<IJsonValue> query)
        {
            var jsonSchema = BuildJsonSchema(context, schema);

            return jsonSchema.Convert(query);
        }

        private ClrQuery ParseJson(Context context, ISchemaEntity? schema, string json)
        {
            var jsonSchema = BuildJsonSchema(context, schema);

            return jsonSchema.Parse(json, jsonSerializer);
        }

        private ClrQuery ParseOData(Context context, ISchemaEntity? schema, string odata)
        {
            try
            {
                var model = BuildEdmModel(context, schema);

                return model.ParseQuery(odata).ToQuery();
            }
            catch (NotSupportedException)
            {
                throw new ValidationException(T.Get("common.odataNotSupported"));
            }
            catch (ODataException ex)
            {
                throw new ValidationException(T.Get("common.odataFailure", new { message = ex.Message }), ex);
            }
        }

        private JsonSchema BuildJsonSchema(Context context, ISchemaEntity? schema)
        {
            if (schema == null)
            {
                return genericJsonSchema;
            }

            var cacheKey = BuildJsonCacheKey(context.App, schema, context.IsFrontendClient);

            var result = cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTime;

                return BuildJsonSchema(schema.SchemaDef, context.App, context.IsFrontendClient);
            });

            return result;
        }

        private IEdmModel BuildEdmModel(Context context, ISchemaEntity? schema)
        {
            if (schema == null)
            {
                return genericEdmModel;
            }

            var cacheKey = BuildEmdCacheKey(context.App, schema, context.IsFrontendClient);

            var result = cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTime;

                return BuildEdmModel(schema.SchemaDef, context.App, context.IsFrontendClient);
            });

            return result;
        }

        private static JsonSchema BuildJsonSchema(Schema schema, IAppEntity app, bool withHiddenFields)
        {
            var dataSchema = schema.BuildJsonSchema(app.PartitionResolver(), (n, s) => s, withHiddenFields);

            return BuildJsonSchema(schema.DisplayName(), dataSchema);
        }

        private static JsonSchema BuildJsonSchema(string name, JsonSchema? dataSchema)
        {
            var schema = new JsonSchema
            {
                Properties =
                {
                    ["id"] = SchemaBuilder.StringProperty($"The id of the {name} content.", true),
                    ["version"] = SchemaBuilder.NumberProperty($"The version of the {name}.", true),
                    ["created"] = SchemaBuilder.DateTimeProperty($"The date and time when the {name} content has been created.", true),
                    ["createdBy"] = SchemaBuilder.StringProperty($"The user that has created the {name} content.", true),
                    ["lastModified"] = SchemaBuilder.DateTimeProperty($"The date and time when the {name} content has been modified last.", true),
                    ["lastModifiedBy"] = SchemaBuilder.StringProperty($"The user that has updated the {name} content last.", true),
                    ["newStatus"] = SchemaBuilder.StringProperty($"The new status of the content.", false),
                    ["status"] = SchemaBuilder.StringProperty($"The status of the content.", true)
                },
                Type = JsonObjectType.Object
            };

            if (dataSchema != null)
            {
                schema.Properties["data"] = SchemaBuilder.ObjectProperty(dataSchema, $"The data of the {name}.", true);
                schema.Properties["dataDraft"] = SchemaBuilder.ObjectProperty(dataSchema, $"The draft data of the {name}.");
            }

            return schema;
        }

        private static EdmModel BuildEdmModel(Schema schema, IAppEntity app, bool withHiddenFields)
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

            var schemaType = schema.BuildEdmType(withHiddenFields, app.PartitionResolver(), typeFactory);

            return BuildEdmModel(app.Name.ToPascalCase(), schema.Name, model, schemaType);
        }

        private static EdmModel BuildEdmModel(string modelName, string name, EdmModel model, EdmComplexType? schemaType)
        {
            var entityType = new EdmEntityType(modelName, name);

            entityType.AddStructuralProperty(nameof(IContentEntity.Id).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.Created).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IContentEntity.CreatedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.LastModified).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IContentEntity.LastModifiedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.NewStatus).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.Status).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.Version).ToCamelCase(), EdmPrimitiveTypeKind.Int32);

            if (schemaType != null)
            {
                entityType.AddStructuralProperty("data", new EdmComplexTypeReference(schemaType, false));
                entityType.AddStructuralProperty("dataDraft", new EdmComplexTypeReference(schemaType, false));

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
