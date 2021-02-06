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

        public OperationsBuilder Schema(Schema schema, bool flat)
        {
            var typeName = schema.TypeName();

            var dataSchema = ResolveSchema($"{typeName}Dto", () =>
            {
                return schema.BuildJsonSchema(partitionResolver, ResolveSchema);
            });

            var contentSchema = ResolveSchema($"{typeName}ContentDto", () =>
            {
                if (flat)
                {
                    return schema.CreateContentSchema(dataSchema, true);
                }
                else
                {
                    var flatData = schema.BuildFlatJsonSchema(ResolveSchema);

                    return schema.CreateContentSchema(flatData, true);
                }
            });

            var builder = new OperationsBuilder
            {
                SchemaName = schema.Name,
                SchemaDisplayName = schema.DisplayName(),
                SchemaTypeName = schema.TypeName(),
                ContentSchema = contentSchema,
                DataSchema = dataSchema,
                Parent = this,
            };

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
