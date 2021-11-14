﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
        public string AppName { get; }

        public JsonSchema ChangeStatusSchema { get; }

        public OpenApiDocument OpenApiDocument { get; }

        public OpenApiSchemaResolver OpenApiSchemaResolver { get; }

        internal Builder(IAppEntity app,
            OpenApiDocument document,
            OpenApiSchemaResolver schemaResolver,
            OpenApiSchemaGenerator schemaGenerator)
        {
            AppName = app.Name;

            OpenApiDocument = document;
            OpenApiSchemaResolver = schemaResolver;

            var changeStatusType = typeof(ChangeStatusDto).ToContextualType();

            ChangeStatusSchema = schemaGenerator.GenerateWithReference<JsonSchema>(changeStatusType, schemaResolver);
        }

        public OperationsBuilder Shared()
        {
            var dataSchema = RegisterReference("DataDto", _ =>
            {
                return JsonSchema.CreateAnySchema();
            });

            var contentSchema = RegisterReference("ContentDto", _ =>
            {
                return ContentJsonSchemaBuilder.BuildSchema(dataSchema, true);
            });

            var contentsSchema = RegisterReference("ContentResultDto", _ =>
            {
                return BuildResult(contentSchema);
            });

            var path = $"/content/{AppName}";

            var builder = new OperationsBuilder
            {
                ContentSchema = contentSchema,
                ContentsSchema = contentsSchema,
                DataSchema = dataSchema,
                Path = path,
                Parent = this,
                SchemaDisplayName = "__Shared",
                SchemaName = "__Shared",
                SchemaTypeName = "__Shared"
            };

            var description = "API endpoints for operations across all schemas.";

            OpenApiDocument.Tags.Add(new OpenApiTag { Name = "__Shared", Description = description });

            return builder;
        }

        public OperationsBuilder Schema(Schema schema, PartitionResolver partitionResolver, ResolvedComponents components, bool flat)
        {
            var typeName = schema.TypeName();

            var dataSchema = RegisterReference($"{typeName}DataDto", _ =>
            {
                return schema.BuildJsonSchemaDynamic(partitionResolver, components, CreateReference, false, true);
            });

            var flatDataSchema = RegisterReference($"{typeName}FlatDataDto", _ =>
            {
                return schema.BuildJsonSchemaFlat(partitionResolver, components, CreateReference, false, true);
            });

            var contentSchema = RegisterReference($"{typeName}ContentDto", _ =>
            {
                return ContentJsonSchemaBuilder.BuildSchema(flat ? flatDataSchema : dataSchema, true);
            });

            var contentsSchema = RegisterReference($"{typeName}ContentResultDto", _ =>
            {
                return BuildResult(contentSchema);
            });

            var path = $"/content/{AppName}/{schema.Name}";

            var builder = new OperationsBuilder
            {
                ContentSchema = contentSchema,
                ContentsSchema = contentsSchema,
                DataSchema = dataSchema,
                Path = path,
                Parent = this,
                SchemaDisplayName = schema.DisplayName(),
                SchemaName = schema.Name,
                SchemaTypeName = typeName
            };

            var description = builder.FormatText("API endpoints for schema content items.");

            OpenApiDocument.Tags.Add(new OpenApiTag { Name = schema.DisplayName(), Description = description });

            return builder;
        }

        private JsonSchema RegisterReference(string name, Func<string, JsonSchema> creator)
        {
            name = char.ToUpperInvariant(name[0]) + name[1..];

            var reference = OpenApiDocument.Definitions.GetOrAdd(name, creator);

            return new JsonSchema
            {
                Reference = reference
            };
        }

        private (JsonSchema, JsonSchema?) CreateReference(string name)
        {
            name = char.ToUpperInvariant(name[0]) + name[1..];

            if (OpenApiDocument.Definitions.TryGetValue(name, out var definition))
            {
                var reference = new JsonSchema
                {
                    Reference = definition
                };

                return (reference, null);
            }

            definition = JsonTypeBuilder.Object();

            OpenApiDocument.Definitions.Add(name, definition);

            return (new JsonSchema
            {
                Reference = definition
            }, definition);
        }

        private static JsonSchema BuildResult(JsonSchema contentSchema)
        {
            return new JsonSchema
            {
                AllowAdditionalProperties = false,
                Properties =
                {
                    ["total"] = JsonTypeBuilder.NumberProperty(
                        FieldDescriptions.ContentsTotal, true),
                    ["items"] = JsonTypeBuilder.ArrayProperty(contentSchema,
                        FieldDescriptions.ContentsItems, true)
                },
                Type = JsonObjectType.Object
            };
        }
    }
}
