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
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Queries.OData;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Text;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class AssetQueryParser
    {
        private readonly JsonSchema jsonSchema = BuildJsonSchema();
        private readonly IEdmModel edmModel = BuildEdmModel();
        private readonly IJsonSerializer jsonSerializer;
        private readonly ITagService tagService;
        private readonly AssetOptions options;

        public AssetQueryParser(IJsonSerializer jsonSerializer, ITagService tagService, IOptions<AssetOptions> options)
        {
            this.jsonSerializer = jsonSerializer;

            this.tagService = tagService;

            this.options = options.Value;
        }

        public virtual async Task<Q> ParseAsync(Context context, Q q)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(q, nameof(q));

            using (Telemetry.Activities.StartActivity("AssetQueryParser/ParseAsync"))
            {
                var query = ParseClrQuery(q);

                await TransformTagAsync(context, query);

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

        private ClrQuery ParseClrQuery(Q q)
        {
            var query = q.Query;

            if (!string.IsNullOrWhiteSpace(q?.QueryAsJson))
            {
                query = ParseJson(q.QueryAsJson);
            }
            else if (!string.IsNullOrWhiteSpace(q?.QueryAsOdata))
            {
                query = ParseOData(q.QueryAsOdata);
            }

            return query;
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

        private async Task TransformTagAsync(Context context, ClrQuery query)
        {
            if (query.Filter != null)
            {
                query.Filter = await FilterTagTransformer.TransformAsync(query.Filter, context.App.Id, tagService);
            }
        }

        private ClrQuery ParseJson(string json)
        {
            return jsonSchema.Parse(json, jsonSerializer);
        }

        private ClrQuery ParseOData(string odata)
        {
            try
            {
                return edmModel.ParseQuery(odata).ToQuery();
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

        private static JsonSchema BuildJsonSchema()
        {
            var schema = new JsonSchema { Title = "Asset", Type = JsonObjectType.Object };

            void AddProperty(string name, JsonObjectType type, string? format = null)
            {
                var property = new JsonSchemaProperty { Type = type, Format = format };

                schema.Properties[name.ToCamelCase()] = property;
            }

            AddProperty("id", JsonObjectType.String);
            AddProperty("version", JsonObjectType.Integer);
            AddProperty("created", JsonObjectType.String, JsonFormatStrings.DateTime);
            AddProperty("createdBy", JsonObjectType.String);
            AddProperty("fileHash", JsonObjectType.String);
            AddProperty("fileName", JsonObjectType.String);
            AddProperty("fileSize", JsonObjectType.Integer);
            AddProperty("fileVersion", JsonObjectType.Integer);
            AddProperty("isDeleted", JsonObjectType.Boolean);
            AddProperty("isProtected", JsonObjectType.Boolean);
            AddProperty("lastModified", JsonObjectType.String, JsonFormatStrings.DateTime);
            AddProperty("lastModifiedBy", JsonObjectType.String);
            AddProperty("metadata", JsonObjectType.None);
            AddProperty("mimeType", JsonObjectType.String);
            AddProperty("slug", JsonObjectType.String);
            AddProperty("tags", JsonObjectType.String);
            AddProperty("type", JsonObjectType.String);

            return schema;
        }

        private static IEdmModel BuildEdmModel()
        {
            var entityType = new EdmEntityType("Squidex", "Asset");

            void AddProperty(string name, EdmPrimitiveTypeKind type)
            {
                entityType.AddStructuralProperty(name.ToCamelCase(), type);
            }

            void AddPropertyReference(string name, IEdmTypeReference reference)
            {
                entityType.AddStructuralProperty(name.ToCamelCase(), reference);
            }

            var jsonType = new EdmComplexType("Squidex", "Json", null, false, true);

            AddPropertyReference("Metadata", new EdmComplexTypeReference(jsonType, false));

            AddProperty("id", EdmPrimitiveTypeKind.String);
            AddProperty("version", EdmPrimitiveTypeKind.Int64);
            AddProperty("created", EdmPrimitiveTypeKind.DateTimeOffset);
            AddProperty("createdBy", EdmPrimitiveTypeKind.String);
            AddProperty("fileHash", EdmPrimitiveTypeKind.String);
            AddProperty("fileName", EdmPrimitiveTypeKind.String);
            AddProperty("isDeleted", EdmPrimitiveTypeKind.Boolean);
            AddProperty("isProtected", EdmPrimitiveTypeKind.Boolean);
            AddProperty("fileSize", EdmPrimitiveTypeKind.Int64);
            AddProperty("fileVersion", EdmPrimitiveTypeKind.Int64);
            AddProperty("lastModified", EdmPrimitiveTypeKind.DateTimeOffset);
            AddProperty("lastModifiedBy", EdmPrimitiveTypeKind.String);
            AddProperty("mimeType", EdmPrimitiveTypeKind.String);
            AddProperty("slug", EdmPrimitiveTypeKind.String);
            AddProperty("tags", EdmPrimitiveTypeKind.String);
            AddProperty("type", EdmPrimitiveTypeKind.String);

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("AssetSet", entityType);

            var model = new EdmModel();

            model.AddElement(container);
            model.AddElement(entityType);

            return model;
        }
    }
}
