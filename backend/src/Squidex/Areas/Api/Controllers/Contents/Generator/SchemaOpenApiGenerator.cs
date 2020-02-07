// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NSwag;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.OpenApi;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemaOpenApiGenerator
    {
        private readonly ContentSchemaBuilder schemaBuilder = new ContentSchemaBuilder();
        private readonly OpenApiDocument document;
        private readonly JsonSchema contentSchema;
        private readonly JsonSchema dataSchema;
        private readonly string schemaPath;
        private readonly string schemaName;
        private readonly string schemaType;
        private readonly string appPath;
        private readonly JsonSchema statusSchema;
        private readonly string appName;

        public SchemaOpenApiGenerator(
            OpenApiDocument document,
            string appName,
            string appPath,
            Schema schema,
            SchemaResolver schemaResolver,
            JsonSchema statusSchema,
            PartitionResolver partitionResolver)
        {
            this.document = document;

            this.appName = appName;
            this.appPath = appPath;

            this.statusSchema = statusSchema;

            schemaPath = schema.Name;
            schemaName = schema.DisplayName();
            schemaType = schema.TypeName();

            dataSchema = schemaResolver($"{schemaType}Dto", schema.BuildJsonSchema(partitionResolver, schemaResolver));

            contentSchema = schemaResolver($"{schemaType}ContentDto", schemaBuilder.CreateContentSchema(schema, dataSchema));
        }

        public void GenerateSchemaOperations()
        {
            document.Tags.Add(
                new OpenApiTag
                {
                    Name = schemaName, Description = $"API to manage {schemaName} contents."
                });

            var schemaOperations = new[]
            {
                GenerateSchemaGetsOperation(),
                GenerateSchemaGetOperation(),
                GenerateSchemaCreateOperation(),
                GenerateSchemaUpdateOperation(),
                GenerateSchemaUpdatePatchOperation(),
                GenerateSchemaStatusOperation(),
                GenerateSchemaDeleteOperation()
            };

            foreach (var operation in schemaOperations.SelectMany(x => x.Values).Distinct())
            {
                operation.Tags = new List<string> { schemaName };
            }
        }

        private OpenApiPathItem GenerateSchemaGetsOperation()
        {
            return Add(OpenApiOperationMethod.Get, Permissions.AppContentsRead, "/",
                operation =>
            {
                operation.OperationId = $"Query{schemaType}Contents";

                operation.Summary = $"Queries {schemaName} contents.";

                operation.Description = NSwagHelper.SchemaQueryDocs;

                operation.AddOData("contents", true);

                operation.AddResponse("200", $"{schemaName} contents retrieved.", CreateContentsSchema(schemaName, contentSchema));
                operation.AddResponse("400", $"{schemaName} query not valid.");
            });
        }

        private OpenApiPathItem GenerateSchemaGetOperation()
        {
            return Add(OpenApiOperationMethod.Get, Permissions.AppContentsRead, "/{id}", operation =>
            {
                operation.OperationId = $"Get{schemaType}Content";

                operation.Summary = $"Get a {schemaName} content.";

                operation.AddResponse("200", $"{schemaName} content found.", contentSchema);
            });
        }

        private OpenApiPathItem GenerateSchemaCreateOperation()
        {
            return Add(OpenApiOperationMethod.Post, Permissions.AppContentsCreate, "/",
                operation =>
            {
                operation.OperationId = $"Create{schemaType}Content";

                operation.Summary = $"Create a {schemaName} content.";

                operation.AddBody("data", dataSchema, NSwagHelper.SchemaBodyDocs);
                operation.AddQuery("publish", JsonObjectType.Boolean, "True to automatically publish the content.");

                operation.AddResponse("201", $"{schemaName} content created.", contentSchema);
                operation.AddResponse("400", $"{schemaName} content not valid.");
            });
        }

        private OpenApiPathItem GenerateSchemaUpdateOperation()
        {
            return Add(OpenApiOperationMethod.Put, Permissions.AppContentsUpdate, "/{id}",
                operation =>
            {
                operation.OperationId = $"Update{schemaType}Content";

                operation.Summary = $"Update a {schemaName} content.";

                operation.AddBody("data", dataSchema, NSwagHelper.SchemaBodyDocs);

                operation.AddResponse("200", $"{schemaName} content updated.", contentSchema);
                operation.AddResponse("400", $"{schemaName} content not valid.");
            });
        }

        private OpenApiPathItem GenerateSchemaUpdatePatchOperation()
        {
            return Add(OpenApiOperationMethod.Patch, Permissions.AppContentsUpdate, "/{id}",
                operation =>
            {
                operation.OperationId = $"Path{schemaType}Content";

                operation.Summary = $"Patch a {schemaName} content.";

                operation.AddBody("data", dataSchema, NSwagHelper.SchemaBodyDocs);

                operation.AddResponse("200", $"{schemaName} content patched.", contentSchema);
                operation.AddResponse("400", $"{schemaName} status not valid.");
            });
        }

        private OpenApiPathItem GenerateSchemaStatusOperation()
        {
            return Add(OpenApiOperationMethod.Put, Permissions.AppContentsUpdate, "/{id}/status",
                operation =>
            {
                operation.OperationId = $"Change{schemaType}ContentStatus";

                operation.Summary = $"Change status of {schemaName} content.";

                operation.AddBody("request", statusSchema, "The request to change content status.");

                operation.AddResponse("200", $"{schemaName} content status changed.", contentSchema);
                operation.AddResponse("400", $"{schemaName} content not valid.");
            });
        }

        private OpenApiPathItem GenerateSchemaDeleteOperation()
        {
            return Add(OpenApiOperationMethod.Delete, Permissions.AppContentsDelete, "/{id}",
                operation =>
            {
                operation.OperationId = $"Delete{schemaType}Content";

                operation.Summary = $"Delete a {schemaName} content.";

                operation.AddResponse("204", $"{schemaName} content deleted.");
            });
        }

        private OpenApiPathItem Add(string method, string permission, string path, Action<OpenApiOperation> updater)
        {
            var operations = document.Paths.GetOrAddNew($"{appPath}/{schemaPath}{path}");
            var operation = new OpenApiOperation
            {
                Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [Constants.SecurityDefinition] = new[]
                        {
                            Permissions.ForApp(permission, appName, schemaPath).Id
                        }
                    }
                }
            };

            updater(operation);

            operations[method] = operation;

            if (path.StartsWith("/{id}", StringComparison.OrdinalIgnoreCase))
            {
                operation.AddPathParameter("id", JsonObjectType.String, $"The id of the {schemaName} content.", JsonFormatStrings.Guid);

                operation.AddResponse("404", $"App, schema or {schemaName} content not found.");
            }

            return operations;
        }

        private static JsonSchema CreateContentsSchema(string schemaName, JsonSchema contentSchema)
        {
            var schema = new JsonSchema
            {
                Properties =
                {
                    ["total"] = new JsonSchemaProperty
                    {
                        Type = JsonObjectType.Number, IsRequired = true, Description = $"The total number of {schemaName} contents."
                    },
                    ["items"] = new JsonSchemaProperty
                    {
                        Type = JsonObjectType.Array, IsRequired = true, Description = $"The {schemaName} contents.", Item = contentSchema
                    }
                },
                Type = JsonObjectType.Object
            };

            return schema;
        }
    }
}