// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Queries.OData;

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
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(tagService, nameof(tagService));

            this.jsonSerializer = jsonSerializer;
            this.options = options.Value;
            this.tagService = tagService;
        }

        public virtual ClrQuery ParseQuery(Context context, Q q)
        {
            Guard.NotNull(context, nameof(context));

            using (Profiler.TraceMethod<AssetQueryParser>())
            {
                var result = new ClrQuery();

                if (!string.IsNullOrWhiteSpace(q?.JsonQuery))
                {
                    result = ParseJson(q?.JsonQuery);
                }
                else if (!string.IsNullOrWhiteSpace(q?.ODataQuery))
                {
                    result = ParseOData(q.ODataQuery);
                }

                if (result.Filter != null)
                {
                    result.Filter = FilterTagTransformer.Transform(result.Filter, context.App.Id, tagService);
                }

                if (result.Sort.Count == 0)
                {
                    result.Sort.Add(new SortNode(new List<string> { "lastModified" }, SortOrder.Descending));
                }

                if (result.Take == long.MaxValue)
                {
                    result.Take = options.DefaultPageSize;
                }
                else if (result.Take > options.MaxResults)
                {
                    result.Take = options.MaxResults;
                }

                return result;
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
            catch (NotSupportedException)
            {
                throw new ValidationException("OData operation is not supported.");
            }
            catch (ODataException ex)
            {
                throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
            }
        }

        private static JsonSchema BuildJsonSchema()
        {
            var schema = new JsonSchema { Title = "Asset", Type = JsonObjectType.Object };

            void AddProperty(string name, JsonObjectType type, string format = null)
            {
                var property = new JsonSchemaProperty { Type = type, Format = format };

                schema.Properties[name.ToCamelCase()] = property;
            }

            AddProperty(nameof(IAssetEntity.Id), JsonObjectType.String, JsonFormatStrings.Guid);
            AddProperty(nameof(IAssetEntity.Created), JsonObjectType.String, JsonFormatStrings.DateTime);
            AddProperty(nameof(IAssetEntity.CreatedBy), JsonObjectType.String);
            AddProperty(nameof(IAssetEntity.LastModified), JsonObjectType.String, JsonFormatStrings.DateTime);
            AddProperty(nameof(IAssetEntity.LastModifiedBy), JsonObjectType.String);
            AddProperty(nameof(IAssetEntity.Version), JsonObjectType.Integer);
            AddProperty(nameof(IAssetEntity.FileName), JsonObjectType.String);
            AddProperty(nameof(IAssetEntity.FileHash), JsonObjectType.String);
            AddProperty(nameof(IAssetEntity.FileSize), JsonObjectType.Integer);
            AddProperty(nameof(IAssetEntity.FileVersion), JsonObjectType.Integer);
            AddProperty(nameof(IAssetEntity.IsImage), JsonObjectType.Boolean);
            AddProperty(nameof(IAssetEntity.MimeType), JsonObjectType.String);
            AddProperty(nameof(IAssetEntity.PixelHeight), JsonObjectType.Integer);
            AddProperty(nameof(IAssetEntity.PixelWidth), JsonObjectType.Integer);
            AddProperty(nameof(IAssetEntity.Slug), JsonObjectType.String);
            AddProperty(nameof(IAssetEntity.Tags), JsonObjectType.String);

            return schema;
        }

        private static IEdmModel BuildEdmModel()
        {
            var entityType = new EdmEntityType("Squidex", "Asset");

            void AddProperty(string name, EdmPrimitiveTypeKind type)
            {
                entityType.AddStructuralProperty(name.ToCamelCase(), type);
            }

            AddProperty(nameof(IAssetEntity.Id), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.Created), EdmPrimitiveTypeKind.DateTimeOffset);
            AddProperty(nameof(IAssetEntity.CreatedBy), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.LastModified), EdmPrimitiveTypeKind.DateTimeOffset);
            AddProperty(nameof(IAssetEntity.LastModifiedBy), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.Version), EdmPrimitiveTypeKind.Int64);
            AddProperty(nameof(IAssetEntity.FileName), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.FileHash), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.FileSize), EdmPrimitiveTypeKind.Int64);
            AddProperty(nameof(IAssetEntity.FileVersion), EdmPrimitiveTypeKind.Int64);
            AddProperty(nameof(IAssetEntity.IsImage), EdmPrimitiveTypeKind.Boolean);
            AddProperty(nameof(IAssetEntity.MimeType), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.PixelHeight), EdmPrimitiveTypeKind.Int32);
            AddProperty(nameof(IAssetEntity.PixelWidth), EdmPrimitiveTypeKind.Int32);
            AddProperty(nameof(IAssetEntity.Slug), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.Tags), EdmPrimitiveTypeKind.String);

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("AssetSet", entityType);

            var model = new EdmModel();

            model.AddElement(container);
            model.AddElement(entityType);

            return model;
        }
    }
}
