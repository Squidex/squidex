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
        private static readonly string SchemaQueryDescription;
        private static readonly string SchemaBodyDescription;
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

        static SchemaOpenApiGenerator()
        {
            SchemaBodyDescription = NSwagHelper.LoadDocs("schemabody");
            SchemaQueryDescription = NSwagHelper.LoadDocs("schemaquery");
        }

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
                    Name = schemaName, Description = $"API to managed {schemaName} contents."
                });

            var schemaOperations = new List<OpenApiPathItem>
            {
                GenerateSchemaGetsOperation(),
                GenerateSchemaGetOperation(),
                GenerateSchemaCreateOperation(),
                GenerateSchemaUpdateOperation(),
                GenerateSchemaUpdatePatchOperation(),
                GenerateSchemaStatusOperation(),
                GenerateSchemaDiscardOperation(),
                GenerateSchemaDeleteOperation()
            };

            foreach (var operation in schemaOperations.SelectMany(x => x.Values).Distinct())
            {
                operation.Tags = new List<string> { schemaName };
            }
        }

        private OpenApiPathItem GenerateSchemaGetsOperation()
        {
            return AddOperation(OpenApiOperationMethod.Get, null, $"{appPath}/{schemaPath}", operation =>
            {
                operation.OperationId = $"Query{schemaType}Contents";

                operation.Summary = $"Queries {schemaName} contents.";

                operation.Description = SchemaQueryDescription;

                operation.AddQueryParameter("$top", JsonObjectType.Number, "Optional number of contents to take (Default: 20).");
                operation.AddQueryParameter("$skip", JsonObjectType.Number, "Optional number of contents to skip.");
                operation.AddQueryParameter("$filter", JsonObjectType.String, "Optional OData filter.");
                operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                operation.AddQueryParameter("$orderby", JsonObjectType.String, "Optional OData order definition.");
                operation.AddQueryParameter("$orderby", JsonObjectType.String, "Optional OData order definition.");

                operation.AddResponse("200", $"{schemaName} content retrieved.", CreateContentsSchema(schemaName, contentSchema));

                AddSecurity(operation, Permissions.AppContentsRead);
            });
        }

        private OpenApiPathItem GenerateSchemaGetOperation()
        {
            return AddOperation(OpenApiOperationMethod.Get, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Get{schemaType}Content";

                operation.Summary = $"Get a {schemaName} content.";

                operation.AddResponse("200", $"{schemaName} content found.", contentSchema);

                AddSecurity(operation, Permissions.AppContentsRead);
            });
        }

        private OpenApiPathItem GenerateSchemaCreateOperation()
        {
            return AddOperation(OpenApiOperationMethod.Post, null, $"{appPath}/{schemaPath}", operation =>
            {
                operation.OperationId = $"Create{schemaType}Content";

                operation.Summary = $"Create a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);
                operation.AddQueryParameter("publish", JsonObjectType.Boolean, "Set to true to autopublish content.");

                operation.AddResponse("201", $"{schemaName} content created.", contentSchema);
                operation.AddResponse("400", $"{schemaName} content not valid.");

                AddSecurity(operation, Permissions.AppContentsCreate);
            });
        }

        private OpenApiPathItem GenerateSchemaUpdateOperation()
        {
            return AddOperation(OpenApiOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Update{schemaType}Content";

                operation.Summary = $"Update a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);

                operation.AddResponse("200", $"{schemaName} content updated.", contentSchema);
                operation.AddResponse("400", $"{schemaName} content not valid.");

                AddSecurity(operation, Permissions.AppContentsUpdate);
            });
        }

        private OpenApiPathItem GenerateSchemaUpdatePatchOperation()
        {
            return AddOperation(OpenApiOperationMethod.Patch, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Path{schemaType}Content";

                operation.Summary = $"Patch a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);

                operation.AddResponse("200", $"{schemaName} content patched.", contentSchema);
                operation.AddResponse("400", $"{schemaName} status not valid.");

                AddSecurity(operation, Permissions.AppContentsUpdate);
            });
        }

        private OpenApiPathItem GenerateSchemaStatusOperation()
        {
            return AddOperation(OpenApiOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/status", operation =>
            {
                operation.OperationId = $"Change{schemaType}ContentStatus";

                operation.Summary = $"Change status of {schemaName} content.";

                operation.AddBodyParameter("request", statusSchema, "The request to change content status.");

                operation.AddResponse("204", $"{schemaName} content status changed.", contentSchema);
                operation.AddResponse("400", $"{schemaName} content not valid.");

                AddSecurity(operation, Permissions.AppContentsUpdate);
            });
        }

        private OpenApiPathItem GenerateSchemaDiscardOperation()
        {
            return AddOperation(OpenApiOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/discard", operation =>
            {
                operation.OperationId = $"Discard{schemaType}Content";

                operation.Summary = $"Discard changes of {schemaName} content.";

                operation.AddResponse("200", $"{schemaName} content status changed.", contentSchema);
                operation.AddResponse("400", $"{schemaName} content has no pending draft.");

                AddSecurity(operation, Permissions.AppContentsDraftDiscard);
            });
        }

        private OpenApiPathItem GenerateSchemaDeleteOperation()
        {
            return AddOperation(OpenApiOperationMethod.Delete, schemaName, $"{appPath}/{schemaPath}/{{id}}/", operation =>
            {
                operation.OperationId = $"Delete{schemaType}Content";

                operation.Summary = $"Delete a {schemaName} content.";

                operation.AddResponse("204", $"{schemaName} content deleted.");

                AddSecurity(operation, Permissions.AppContentsDelete);
            });
        }

        private OpenApiPathItem AddOperation(string method, string entityName, string path, Action<OpenApiOperation> updater)
        {
            var operations = document.Paths.GetOrAddNew(path);
            var operation = new OpenApiOperation();

            updater(operation);

            operations[method] = operation;

            if (entityName != null)
            {
                operation.AddPathParameter("id", JsonObjectType.String, $"The id of the {entityName} content (GUID).");

                operation.AddResponse("404", $"App, schema or {entityName} content not found.");
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
                        Type = JsonObjectType.Array, IsRequired = true, Item = contentSchema, Description = $"The {schemaName} contents."
                    }
                },
                Type = JsonObjectType.Object
            };

            return schema;
        }

        private void AddSecurity(OpenApiOperation operation, string permission)
        {
            if (operation.Security == null)
            {
                operation.Security = new List<OpenApiSecurityRequirement>();
            }

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [Constants.SecurityDefinition] = new[] { Permissions.ForApp(permission, appName, schemaPath).Id }
            });
        }
    }
}