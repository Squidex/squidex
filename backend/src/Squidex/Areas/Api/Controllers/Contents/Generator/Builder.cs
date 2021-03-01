// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Namotion.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    internal sealed class Builder
    {
        private readonly PartitionResolver partitionResolver;

        public string AppName { get; }

        public JsonSchema ChangeStatusSchema { get; }

        public OpenApiDocument Document { get; }

        internal Builder(IAppEntity app,
            OpenApiDocument document,
            OpenApiSchemaResolver schemaResolver,
            OpenApiSchemaGenerator schemaGenerator)
        {
            this.partitionResolver = app.PartitionResolver();

            Document = document;

            AppName = app.Name;

            ChangeStatusSchema = schemaGenerator.GenerateWithReference<JsonSchema>(typeof(ChangeStatusDto).ToContextualType(), schemaResolver);
        }

        public OperationsBuilder Shared()
        {
            var dataSchema = ResolveSchema("DataDto", () =>
            {
                return JsonSchema.CreateAnySchema();
            });

            var contentSchema = ResolveSchema($"ContentDto", () =>
            {
                return ContentJsonSchemaBuilder.BuildSchema("Shared", dataSchema, true);
            });

            var path = $"/content/{AppName}";

            var builder = new OperationsBuilder
            {
                ContentSchema = contentSchema,
                DataSchema = dataSchema,
                Path = path,
                Parent = this,
                SchemaDisplayName = "__Shared",
                SchemaName = "__Shared",
                SchemaTypeName = "__Shared",
            };

            var description = "API endpoints for operations across all schemas.";

            Document.Tags.Add(new OpenApiTag { Name = "__Shared", Description = description });

            return builder;
        }

        public OperationsBuilder Schema(Schema schema, bool flat)
        {
            var typeName = schema.TypeName();

            var displayName = schema.DisplayName();

            var dataSchema = ResolveSchema($"{typeName}DataDto", () =>
            {
                return schema.BuildJsonSchema(partitionResolver, ResolveSchema);
            });

            var dataFlatSchema = ResolveSchema($"{typeName}FlatDataDto", () =>
            {
                return schema.BuildFlatJsonSchema(ResolveSchema);
            });

            var contentSchema = ResolveSchema($"{typeName}ContentDto", () =>
            {
                var data = flat ? dataFlatSchema : dataSchema;

                return ContentJsonSchemaBuilder.BuildSchema(displayName, data, true);
            });

            var path = $"/content/{AppName}/{schema.Name}";

            var builder = new OperationsBuilder
            {
                ContentSchema = contentSchema,
                DataSchema = dataSchema,
                Path = path,
                Parent = this,
                SchemaDisplayName = displayName,
                SchemaName = schema.Name,
                SchemaTypeName = typeName
            };

            var description = builder.FormatText("API endpoints for schema content items.");

            Document.Tags.Add(new OpenApiTag { Name = displayName, Description = description });

            return builder;
        }

        private JsonSchema ResolveSchema(string name, Func<JsonSchema> factory)
        {
            name = char.ToUpperInvariant(name[0]) + name[1..];

            return new JsonSchema
            {
                Reference = Document.Definitions.GetOrAdd(name, x => factory())
            };
        }
    }
}
