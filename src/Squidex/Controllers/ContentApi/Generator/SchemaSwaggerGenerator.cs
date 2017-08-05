// ==========================================================================
//  SchemaSwaggerGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NSwag;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Contents.JsonSchema;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;

// ReSharper disable InvertIf

namespace Squidex.Controllers.ContentApi.Generator
{
    public sealed class SchemaSwaggerGenerator
    {
        private static readonly string schemaQueryDescription;
        private static readonly string schemaBodyDescription;
        private readonly ContentSchemaBuilder schemaBuilder = new ContentSchemaBuilder();
        private readonly SwaggerDocument document;
        private readonly JsonSchema4 contentSchema;
        private readonly JsonSchema4 dataSchema;
        private readonly string schemaPath;
        private readonly string schemaName;
        private readonly string schemaKey;
        private readonly string appPath;

        static SchemaSwaggerGenerator()
        {
            schemaBodyDescription = SwaggerHelper.LoadDocs("schemabody");
            schemaQueryDescription = SwaggerHelper.LoadDocs("schemaquery");
        }

        public SchemaSwaggerGenerator(SwaggerDocument document, string path, Schema schema,
            Func<string, JsonSchema4, JsonSchema4> schemaResolver, PartitionResolver partitionResolver)
        {
            this.document = document;

            appPath = path;

            schemaPath = schema.Name;
            schemaName = schema.Properties.Label.WithFallback(schema.Name);
            schemaKey = schema.Name.ToPascalCase();

            dataSchema = schemaResolver($"{schemaKey}Dto", schema.BuildJsonSchema(partitionResolver, schemaResolver));

            contentSchema = schemaResolver($"{schemaKey}ContentDto", schemaBuilder.CreateContentSchema(schema, dataSchema));
        }

        public void GenerateSchemaOperations()
        {
            document.Tags.Add(
                new SwaggerTag
                {
                    Name = schemaName, Description = $"API to managed {schemaName} contents."
                });

            var schemaOperations = new List<SwaggerOperations>
            {
                GenerateSchemaQueryOperation(),
                GenerateSchemaCreateOperation(),
                GenerateSchemaGetOperation(),
                GenerateSchemaUpdateOperation(),
                GenerateSchemaPatchOperation(),
                GenerateSchemaPublishOperation(),
                GenerateSchemaUnpublishOperation(),
                GenerateSchemaDeleteOperation()
            };

            foreach (var operation in schemaOperations.SelectMany(x => x.Values).Distinct())
            {
                operation.Tags = new List<string> { schemaName };
            }
        }

        private SwaggerOperations GenerateSchemaQueryOperation()
        {
            return AddOperation(SwaggerOperationMethod.Get, null, $"{appPath}/{schemaPath}", operation =>
            {
                operation.OperationId = $"Query{schemaKey}Contents";
                operation.Summary = $"Queries {schemaName} contents.";

                operation.Description = schemaQueryDescription;

                operation.AddQueryParameter("$top", JsonObjectType.Number, "Optional number of contents to take.");
                operation.AddQueryParameter("$skip", JsonObjectType.Number, "Optional number of contents to skip.");
                operation.AddQueryParameter("$filter", JsonObjectType.String, "Optional OData filter.");
                operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                operation.AddQueryParameter("orderby", JsonObjectType.String, "Optional OData order definition.");

                operation.AddResponse("200", $"{schemaName} content retrieved.", CreateContentsSchema(schemaName, contentSchema));
            });
        }

        private SwaggerOperations GenerateSchemaGetOperation()
        {
            return AddOperation(SwaggerOperationMethod.Get, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Get{schemaKey}Content";
                operation.Summary = $"Get a {schemaName} content.";

                operation.AddResponse("200", $"{schemaName} content found.", contentSchema);
            });
        }

        private SwaggerOperations GenerateSchemaCreateOperation()
        {
            return AddOperation(SwaggerOperationMethod.Post, null, $"{appPath}/{schemaPath}", operation =>
            {
                operation.OperationId = $"Create{schemaKey}Content";
                operation.Summary = $"Create a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, schemaBodyDescription);
                operation.AddQueryParameter("publish", JsonObjectType.Boolean, "Set to true to autopublish content.");

                operation.AddResponse("201", $"{schemaName} created.", contentSchema);
            });
        }

        private SwaggerOperations GenerateSchemaUpdateOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Update{schemaKey}Content";
                operation.Summary = $"Update a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, schemaBodyDescription);

                operation.AddResponse("204", $"{schemaName} element updated.");
            });
        }

        private SwaggerOperations GenerateSchemaPatchOperation()
        {
            return AddOperation(SwaggerOperationMethod.Patch, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Path{schemaKey}Content";
                operation.Summary = $"Patchs a {schemaName} content.";

                operation.AddBodyParameter("data", contentSchema, schemaBodyDescription);

                operation.AddResponse("204", $"{schemaName} element updated.");
            });
        }

        private SwaggerOperations GenerateSchemaPublishOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/publish", operation =>
            {
                operation.OperationId = $"Publish{schemaKey}Content";
                operation.Summary = $"Publish a {schemaName} content.";

                operation.AddResponse("204", $"{schemaName} element published.");
            });
        }

        private SwaggerOperations GenerateSchemaUnpublishOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/unpublish", operation =>
            {
                operation.OperationId = $"Unpublish{schemaKey}Content";
                operation.Summary = $"Unpublish a {schemaName} content.";

                operation.AddResponse("204", $"{schemaName} element unpublished.");
            });
        }

        private SwaggerOperations GenerateSchemaDeleteOperation()
        {
            return AddOperation(SwaggerOperationMethod.Delete, schemaName, $"{appPath}/{schemaPath}/{{id}}/", operation =>
            {
                operation.OperationId = $"Delete{schemaKey}Content";
                operation.Summary = $"Delete a {schemaName} content.";

                operation.AddResponse("204", $"{schemaName} content deleted.");
            });
        }

        private SwaggerOperations AddOperation(SwaggerOperationMethod method, string entityName, string path, Action<SwaggerOperation> updater)
        {
            var operations = document.Paths.GetOrAdd(path, k => new SwaggerOperations());
            var operation = new SwaggerOperation();

            updater(operation);

            operations[method] = operation;

            if (entityName != null)
            {
                operation.AddPathParameter("id", JsonObjectType.String, $"The id of the {entityName} content (GUID).");

                operation.AddResponse("404", $"App, schema or {entityName} content not found.");
            }

            return operations;
        }

        private static JsonSchema4 CreateContentsSchema(string schemaName, JsonSchema4 contentSchema)
        {
            var schema = new JsonSchema4
            {
                Properties =
                {
                    ["total"] = new JsonProperty
                    {
                        Type = JsonObjectType.Number, IsRequired = true, Description = $"The total number of {schemaName} contents."
                    },
                    ["items"] = new JsonProperty
                    {
                        Type = JsonObjectType.Array, IsRequired = true, Item = contentSchema, Description = $"The {schemaName} contents."
                    }
                },
                Type = JsonObjectType.Object
            };

            return schema;
        }
    }
}