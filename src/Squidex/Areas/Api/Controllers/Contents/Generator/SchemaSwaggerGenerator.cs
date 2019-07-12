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
using Squidex.Pipeline.Swagger;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemaSwaggerGenerator
    {
        private static readonly string SchemaQueryDescription;
        private static readonly string SchemaBodyDescription;
        private readonly ContentSchemaBuilder schemaBuilder = new ContentSchemaBuilder();
        private readonly SwaggerDocument document;
        private readonly JsonSchema4 contentSchema;
        private readonly JsonSchema4 dataSchema;
        private readonly string schemaPath;
        private readonly string schemaName;
        private readonly string schemaType;
        private readonly string appPath;
        private readonly JsonSchema4 statusSchema;
        private readonly string appName;

        static SchemaSwaggerGenerator()
        {
            SchemaBodyDescription = NSwagHelper.LoadDocs("schemabody");
            SchemaQueryDescription = NSwagHelper.LoadDocs("schemaquery");
        }

        public SchemaSwaggerGenerator(
            SwaggerDocument document,
            string appName,
            string appPath,
            Schema schema,
            SchemaResolver schemaResolver,
            JsonSchema4 statusSchema,
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
                new SwaggerTag
                {
                    Name = schemaName, Description = $"API to managed {schemaName} contents."
                });

            var schemaOperations = new List<SwaggerPathItem>
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

        private SwaggerPathItem GenerateSchemaGetsOperation()
        {
            return AddOperation(SwaggerOperationMethod.Get, null, $"{appPath}/{schemaPath}", operation =>
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

        private SwaggerPathItem GenerateSchemaGetOperation()
        {
            return AddOperation(SwaggerOperationMethod.Get, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Get{schemaType}Content";

                operation.Summary = $"Get a {schemaName} content.";

                operation.AddResponse("200", $"{schemaName} content found.", contentSchema);

                AddSecurity(operation, Permissions.AppContentsRead);
            });
        }

        private SwaggerPathItem GenerateSchemaCreateOperation()
        {
            return AddOperation(SwaggerOperationMethod.Post, null, $"{appPath}/{schemaPath}", operation =>
            {
                operation.OperationId = $"Create{schemaType}Content";

                operation.Summary = $"Create a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);
                operation.AddQueryParameter("publish", JsonObjectType.Boolean, "Set to true to autopublish content.");

                operation.AddResponse("200", $"{schemaName} content created.", contentSchema);
                operation.AddResponse("400", "Content data valid.");

                AddSecurity(operation, Permissions.AppContentsCreate);
            });
        }

        private SwaggerPathItem GenerateSchemaUpdateOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Update{schemaType}Content";

                operation.Summary = $"Update a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);

                operation.AddResponse("200", $"{schemaName} content updated.", contentSchema);
                operation.AddResponse("400", "Content data valid.");

                AddSecurity(operation, Permissions.AppContentsUpdate);
            });
        }

        private SwaggerPathItem GenerateSchemaUpdatePatchOperation()
        {
            return AddOperation(SwaggerOperationMethod.Patch, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Path{schemaType}Content";

                operation.Summary = $"Patch a {schemaName} content.";

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);

                operation.AddResponse("200", $"{schemaName} content patched.", contentSchema);
                operation.AddResponse("400", "Status change not valid.");

                AddSecurity(operation, Permissions.AppContentsUpdate);
            });
        }

        private SwaggerPathItem GenerateSchemaStatusOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/status", operation =>
            {
                operation.OperationId = $"Change{schemaType}ContentStatus";

                operation.Summary = $"Change status of {schemaName} content.";

                operation.AddBodyParameter("request", statusSchema, "The request to change content status.");

                operation.AddResponse("204", $"{schemaName} content status changed.", contentSchema);
                operation.AddResponse("400", "Content data valid.");

                AddSecurity(operation, Permissions.AppContentsUpdate);
            });
        }

        private SwaggerPathItem GenerateSchemaDiscardOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/discard", operation =>
            {
                operation.OperationId = $"Discard{schemaType}Content";

                operation.Summary = $"Discard changes of {schemaName} content.";

                operation.AddResponse("400", "No pending draft.");
                operation.AddResponse("200", $"{schemaName} content status changed.", contentSchema);

                AddSecurity(operation, Permissions.AppContentsDraftDiscard);
            });
        }

        private SwaggerPathItem GenerateSchemaDeleteOperation()
        {
            return AddOperation(SwaggerOperationMethod.Delete, schemaName, $"{appPath}/{schemaPath}/{{id}}/", operation =>
            {
                operation.OperationId = $"Delete{schemaType}Content";

                operation.Summary = $"Delete a {schemaName} content.";

                operation.AddResponse("204", $"{schemaName} content deleted.");

                AddSecurity(operation, Permissions.AppContentsDelete);
            });
        }

        private SwaggerPathItem AddOperation(string method, string entityName, string path, Action<SwaggerOperation> updater)
        {
            var operations = document.Paths.GetOrAddNew(path);
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

        private void AddSecurity(SwaggerOperation operation, string permission)
        {
            if (operation.Security == null)
            {
                operation.Security = new List<SwaggerSecurityRequirement>();
            }

            operation.Security.Add(new SwaggerSecurityRequirement
            {
                [Constants.SecurityDefinition] = new[] { Permissions.ForApp(permission, appName, schemaPath).Id }
            });
        }
    }
}