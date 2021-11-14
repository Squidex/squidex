// ==========================================================================
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

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    internal sealed class Builder : SchemaResolver
    {
        private const string ResultTotal = "total";
        private const string ResultItems = "items";
        private readonly Dictionary<string, (Func<JsonSchema> Creator, List<JsonSchema> References)> factories = new Dictionary<string, (Func<JsonSchema>, List<JsonSchema>)>();

        public string AppName { get; }

        public JsonSchema ChangeStatusSchema { get; }

        public OpenApiDocument OpenApiDocument { get; }

        public OpenApiSchemaResolver OpenApiSchemaResolver { get; }

        public override bool ProvidesComponents => true;

        internal Builder(IAppEntity app,
            OpenApiDocument document,
            OpenApiSchemaResolver schemaResolver,
            OpenApiSchemaGenerator schemaGenerator)
        {
            AppName = app.Name;

            OpenApiDocument = document;
            OpenApiSchemaResolver = schemaResolver;

            ChangeStatusSchema = schemaGenerator.GenerateWithReference<JsonSchema>(typeof(ChangeStatusDto).ToContextualType(), schemaResolver);
        }

        public override JsonSchema Register(JsonSchema schema, string typeName)
        {
            OpenApiSchemaResolver.AppendSchema(schema, typeName);

            return new JsonSchema
            {
                Reference = schema
            };
        }

        public override (string?, JsonSchema?) GetComponent(Schema schema)
        {
            var name = $"{schema.TypeName()}ComponentDto";

            return (name, GetReference(name));
        }

        public void Prepare(Schema schema, ResolvedComponents components, bool flat)
        {
            var typeName = schema.TypeName();

            RegisterFactory($"{typeName}ComponentDto", () =>
            {
                return schema.BuildJsonSchemaForComponent(this, components);
            });

            var dataSchema = RegisterFactory($"{typeName}DataDto", () =>
            {
                return schema.BuildJsonSchemaDynamic(this, components);
            });

            var flatDataSchema = RegisterFactory($"{typeName}FlatDataDto", () =>
            {
                return schema.BuildJsonSchemaFlat(this, components);
            });

            var contentSchema = RegisterFactory($"{typeName}ContentDto", () =>
            {
                return ContentJsonSchemaBuilder.BuildSchema(flat ? flatDataSchema : dataSchema, true);
            });

            RegisterFactory($"{typeName}ContentResultDto", () =>
            {
                return BuildResult(contentSchema);
            });
        }

        public void Complete()
        {
            foreach (var (typeName, (factory, _)) in factories)
            {
                var schema = factory();

                OpenApiDocument.Definitions.Add(typeName, schema);
            }

            foreach (var (typeName, (_, references)) in factories)
            {
                var schema = OpenApiDocument.Definitions[typeName];

                if (references.Count == 0)
                {
                    OpenApiDocument.Definitions.Remove(typeName);
                }

                foreach (var reference in references)
                {
                    reference.Reference = schema;
                }
            }
        }

        public OperationsBuilder Shared()
        {
            var dataSchema = RegisterFactory("DataDto", () =>
            {
                return JsonSchema.CreateAnySchema();
            });

            var contentSchema = RegisterFactory("ContentDto", () =>
            {
                return ContentJsonSchemaBuilder.BuildSchema(dataSchema, true);
            });

            var contentsSchema = RegisterFactory("ContentResultDto", () =>
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

        public OperationsBuilder Schema(Schema schema)
        {
            var typeName = schema.TypeName();

            var path = $"/content/{AppName}/{schema.Name}";

            var builder = new OperationsBuilder
            {
                ContentSchema = GetReference($"{typeName}ContentDto"),
                ContentsSchema = GetReference($"{typeName}ContentResultDto"),
                DataSchema = GetReference($"{typeName}DataDto"),
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

        private JsonSchema GetReference(string name)
        {
            name = char.ToUpperInvariant(name[0]) + name[1..];

            var reference = new JsonSchema();

            factories[name].References.Add(reference);

            return reference;
        }

        private JsonSchema RegisterFactory(string name, Func<JsonSchema> creator)
        {
            name = char.ToUpperInvariant(name[0]) + name[1..];

            if (!factories.TryGetValue(name, out var factory))
            {
                factory = (creator, new List<JsonSchema>());
                factories.Add(name, factory);
            }

            var reference = new JsonSchema();

            factory.References.Add(reference);

            return reference;
        }

        private static JsonSchema BuildResult(JsonSchema contentSchema)
        {
            return new JsonSchema
            {
                AllowAdditionalProperties = false,
                Properties =
                {
                    [ResultTotal] = SchemaBuilder.NumberProperty(
                        FieldDescriptions.ContentsTotal, true),
                    [ResultItems] = SchemaBuilder.ArrayProperty(contentSchema,
                        FieldDescriptions.ContentsItems, true)
                },
                Type = JsonObjectType.Object
            };
        }
    }
}
