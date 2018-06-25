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
using Squidex.Config;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;
using Squidex.Shared.Identity;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemaSwaggerGenerator
    {
        private static readonly string SchemaQueryDescription;
        private static readonly string SchemaBodyDescription;
        private static readonly List<SwaggerSecurityRequirement> EditorSecurity;
        private static readonly List<SwaggerSecurityRequirement> ReaderSecurity;
        private readonly ContentSchemaBuilder schemaBuilder = new ContentSchemaBuilder();
        private readonly SwaggerDocument document;
        private readonly JsonSchema4 contentSchema;
        private readonly JsonSchema4 dataSchema;
        private readonly string schemaPath;
        private readonly string schemaName;
        private readonly string schemaType;
        private readonly string appPath;

        static SchemaSwaggerGenerator()
        {
            SchemaBodyDescription = SwaggerHelper.LoadDocs("schemabody");
            SchemaQueryDescription = SwaggerHelper.LoadDocs("schemaquery");

            ReaderSecurity = new List<SwaggerSecurityRequirement>
            {
                new SwaggerSecurityRequirement
                {
                    {
                        Constants.SecurityDefinition, new[] { SquidexRoles.AppReader }
                    }
                }
            };

            EditorSecurity = new List<SwaggerSecurityRequirement>
            {
                new SwaggerSecurityRequirement
                {
                    {
                        Constants.SecurityDefinition, new[] { SquidexRoles.AppEditor }
                    }
                }
            };
        }

        public SchemaSwaggerGenerator(SwaggerDocument document, string path, Schema schema, Func<string, JsonSchema4, JsonSchema4> schemaResolver, PartitionResolver partitionResolver)
        {
            this.document = document;

            appPath = path;

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
                GenerateSchemaQueryOperation(),
                GenerateSchemaCreateOperation(),
                GenerateSchemaGetOperation(),
                GenerateSchemaUpdateOperation(),
                GenerateSchemaPatchOperation(),
                GenerateSchemaPublishOperation(),
                GenerateSchemaUnpublishOperation(),
                GenerateSchemaArchiveOperation(),
                GenerateSchemaRestoreOperation(),
                GenerateSchemaDeleteOperation()
            };

            foreach (var operation in schemaOperations.SelectMany(x => x.Values).Distinct())
            {
                operation.Tags = new List<string> { schemaName };
            }
        }

        private SwaggerPathItem GenerateSchemaQueryOperation()
        {
            return AddOperation(SwaggerOperationMethod.Get, null, $"{appPath}/{schemaPath}", operation =>
            {
                operation.OperationId = $"Query{schemaType}Contents";
                operation.Summary = $"Queries {schemaName} contents.";
                operation.Security = ReaderSecurity;

                operation.Description = SchemaQueryDescription;

                operation.AddQueryParameter("$top", JsonObjectType.Number, "Optional number of contents to take (Default: 20).");
                operation.AddQueryParameter("$skip", JsonObjectType.Number, "Optional number of contents to skip.");
                operation.AddQueryParameter("$filter", JsonObjectType.String, "Optional OData filter.");
                operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                operation.AddQueryParameter("orderby", JsonObjectType.String, "Optional OData order definition.");

                operation.AddResponse("200", $"{schemaName} content retrieved.", CreateContentsSchema(schemaName, contentSchema));
            });
        }

        private SwaggerPathItem GenerateSchemaGetOperation()
        {
            return AddOperation(SwaggerOperationMethod.Get, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Get{schemaType}Content";
                operation.Summary = $"Get a {schemaName} content.";
                operation.Security = ReaderSecurity;

                operation.AddResponse("200", $"{schemaName} content found.", contentSchema);
            });
        }

        private SwaggerPathItem GenerateSchemaCreateOperation()
        {
            return AddOperation(SwaggerOperationMethod.Post, null, $"{appPath}/{schemaPath}", operation =>
            {
                operation.OperationId = $"Create{schemaType}Content";
                operation.Summary = $"Create a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);
                operation.AddQueryParameter("publish", JsonObjectType.Boolean, "Set to true to autopublish content.");

                operation.AddResponse("201", $"{schemaName} content created.", contentSchema);
            });
        }

        private SwaggerPathItem GenerateSchemaUpdateOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Update{schemaType}Content";
                operation.Summary = $"Update a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);

                operation.AddResponse("200", $"{schemaName} content updated.", dataSchema);
            });
        }

        private SwaggerPathItem GenerateSchemaPatchOperation()
        {
            return AddOperation(SwaggerOperationMethod.Patch, schemaName, $"{appPath}/{schemaPath}/{{id}}", operation =>
            {
                operation.OperationId = $"Path{schemaType}Content";
                operation.Summary = $"Patch a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddBodyParameter("data", dataSchema, SchemaBodyDescription);

                operation.AddResponse("200", $"{schemaName} content patched.", dataSchema);
            });
        }

        private SwaggerPathItem GenerateSchemaPublishOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/publish", operation =>
            {
                operation.OperationId = $"Publish{schemaType}Content";
                operation.Summary = $"Publish a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddResponse("204", $"{schemaName} content published.");
            });
        }

        private SwaggerPathItem GenerateSchemaUnpublishOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/unpublish", operation =>
            {
                operation.OperationId = $"Unpublish{schemaType}Content";
                operation.Summary = $"Unpublish a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddResponse("204", $"{schemaName} content unpublished.");
            });
        }

        private SwaggerPathItem GenerateSchemaArchiveOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/archive", operation =>
            {
                operation.OperationId = $"Archive{schemaType}Content";
                operation.Summary = $"Archive a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddResponse("204", $"{schemaName} content restored.");
            });
        }

        private SwaggerPathItem GenerateSchemaRestoreOperation()
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appPath}/{schemaPath}/{{id}}/restore", operation =>
            {
                operation.OperationId = $"Restore{schemaType}Content";
                operation.Summary = $"Restore a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddResponse("204", $"{schemaName} content restored.");
            });
        }

        private SwaggerPathItem GenerateSchemaDeleteOperation()
        {
            return AddOperation(SwaggerOperationMethod.Delete, schemaName, $"{appPath}/{schemaPath}/{{id}}/", operation =>
            {
                operation.OperationId = $"Delete{schemaType}Content";
                operation.Summary = $"Delete a {schemaName} content.";
                operation.Security = EditorSecurity;

                operation.AddResponse("204", $"{schemaName} content deleted.");
            });
        }

        private SwaggerPathItem AddOperation(SwaggerOperationMethod method, string entityName, string path, Action<SwaggerOperation> updater)
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
    }
}