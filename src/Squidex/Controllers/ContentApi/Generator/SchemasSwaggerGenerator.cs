// ==========================================================================
//  SchemasSwaggerGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration;
using Squidex.Config;
using Squidex.Controllers.Api;
using Squidex.Core.Identity;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;
using Squidex.Read.Apps;
using Squidex.Read.Schemas;

// ReSharper disable InvertIf
// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Squidex.Controllers.ContentApi.Generator
{
    public sealed class SchemasSwaggerGenerator
    {
        private const string BodyDescription =
            @"The data of the {0} to be created or updated.
            
Please note that each field is an object with one entry per language. 
If the field is not localizable you must use iv (Invariant Language) as a key.
When you change the field to be localizable the value will become the value for the master language, depending what the master language is at this point of time.";

        private readonly SwaggerJsonSchemaGenerator schemaGenerator;
        private readonly SwaggerDocument document = new SwaggerDocument { Tags = new List<SwaggerTag>() };
        private readonly HttpContext context;
        private readonly JsonSchemaResolver schemaResolver;
        private readonly SwaggerGenerator swaggerGenerator;
        private readonly MyUrlsOptions urlOptions;
        private HashSet<Language> languages;
        private JsonSchema4 errorDtoSchema;
        private JsonSchema4 entityCreatedDtoSchema;
        private string appBasePath;
        private IAppEntity app;

        public SchemasSwaggerGenerator(IHttpContextAccessor context, SwaggerOwinSettings swaggerSettings, IOptions<MyUrlsOptions> urlOptions)
        {
            this.context = context.HttpContext;

            this.urlOptions = urlOptions.Value;

            schemaGenerator = new SwaggerJsonSchemaGenerator(swaggerSettings);
            schemaResolver = new SwaggerSchemaResolver(document, swaggerSettings);

            swaggerGenerator = new SwaggerGenerator(schemaGenerator, swaggerSettings, schemaResolver);

        }

        public async Task<SwaggerDocument> Generate(IAppEntity appEntity, IEnumerable<ISchemaEntityWithSchema> schemas)
        {
            app = appEntity;

            languages = new HashSet<Language>(appEntity.Languages);

            appBasePath = $"/content/{appEntity.Name}";

            await GenerateBasicSchemas();
            
            GenerateTitle();
            GenerateRequestInfo();
            GenerateContentTypes();
            GenerateSchemes();
            GenerateSchemasOperations(schemas);
            GenerateSecurityDefinitions();
            GenerateSecurityRequirements();
            GenerateDefaultErrors();

            return document;
        }

        private void GenerateSchemes()
        {
            document.Schemes.Add(context.Request.Scheme == "http" ? SwaggerSchema.Http : SwaggerSchema.Https);
        }

        private void GenerateTitle()
        {
            document.Host = context.Request.Host.Value ?? string.Empty;

            document.BasePath = "/api";
        }

        private void GenerateRequestInfo()
        {
            document.Info = new SwaggerInfo
            {
                Title = $"Suidex API for {app.Name} App"
            };
        }

        private void GenerateContentTypes()
        {
            document.Consumes = new List<string>
            {
                "application/json"
            };

            document.Produces = new List<string>
            {
                "application/json"
            };
        }

        private void GenerateSecurityDefinitions()
        {
            document.SecurityDefinitions.Add("OAuth2", SwaggerHelper.CreateOAuthSchema(urlOptions));
        }

        private async Task GenerateBasicSchemas()
        {
            var errorType = typeof(ErrorDto);
            var errorSchema = JsonObjectTypeDescription.FromType(errorType, new Attribute[0], EnumHandling.String);

            errorDtoSchema = await swaggerGenerator.GenerateAndAppendSchemaFromTypeAsync(errorType, errorSchema.IsNullable, null);

            var entityCreatedType = typeof(EntityCreatedDto);
            var entityCreatedSchema = JsonObjectTypeDescription.FromType(entityCreatedType, new Attribute[0], EnumHandling.String);

            entityCreatedDtoSchema = await swaggerGenerator.GenerateAndAppendSchemaFromTypeAsync(entityCreatedType, entityCreatedSchema.IsNullable, null);
        }

        private void GenerateSecurityRequirements()
        {
            var securityRequirements = new List<SwaggerSecurityRequirement>
            {
                new SwaggerSecurityRequirement
                {
                    { "roles", new List<string> { SquidexRoles.AppOwner, SquidexRoles.AppDeveloper, SquidexRoles.AppEditor } }
                }
            };

            foreach (var operation in document.Paths.Values.SelectMany(x => x.Values))
            {
                operation.Security = securityRequirements;
            }
        }

        private void GenerateDefaultErrors()
        {
            foreach (var operation in document.Paths.Values.SelectMany(x => x.Values))
            {
                operation.Responses.Add("500", new SwaggerResponse { Description = "Operation failed with internal server error.", Schema = errorDtoSchema });
            }
        }

        private void GenerateSchemasOperations(IEnumerable<ISchemaEntityWithSchema> schemas)
        {
            foreach (var schema in schemas.Select(x => x.Schema))
            {
                GenerateSchemaOperations(schema);
            }
        }

        private void GenerateSchemaOperations(Schema schema)
        {
            var schemaName = schema.Properties.Label ?? schema.Name;

            document.Tags.Add(
                new SwaggerTag
                {
                    Name = schemaName, Description = $"API to managed {schemaName} content."
                });

            var dataSchem = AppendSchema($"{schema.Name}Dto", schema.BuildSchema(languages, AppendSchema));

            var schemaOperations = new List<SwaggerOperations>
            {
                GenerateSchemaQueryOperation(schema, schemaName, dataSchem),
                GenerateSchemaCreateOperation(schema, schemaName, dataSchem),
                GenerateSchemaGetOperation(schema, schemaName, dataSchem),
                GenerateSchemaUpdateOperation(schema, schemaName, dataSchem),
                GenerateSchemaPatchOperation(schema, schemaName, dataSchem),
                GenerateSchemaPublishOperation(schema, schemaName),
                GenerateSchemaUnpublishOperation(schema, schemaName),
                GenerateSchemaDeleteOperation(schema, schemaName)
            };

            foreach (var operation in schemaOperations.SelectMany(x => x.Values).Distinct())
            {
                operation.Tags = new List<string> { schemaName };
            }
        }

        private SwaggerOperations GenerateSchemaQueryOperation(Schema schema, string schemaName, JsonSchema4 dataSchem)
        {
            return AddOperation(SwaggerOperationMethod.Get, null, $"{appBasePath}/{schema.Name}", operation =>
            {
                operation.Summary = $"Queries {schemaName} content."; 

                operation.AddQueryParameter("$top", JsonObjectType.Number, "Optional number of contents to take.");
                operation.AddQueryParameter("$skip", JsonObjectType.Number, "Optional number of contents to skip.");
                operation.AddQueryParameter("$filter", JsonObjectType.String, "Optional OData filter.");
                operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                operation.AddQueryParameter("orderby", JsonObjectType.String, "Optional OData order definition.");

                var responseSchema = CreateContentsSchema(schemaName, schema.Name, dataSchem);

                operation.AddResponse("200", $"{schemaName} content retrieved.", responseSchema);
            });
        }

        private SwaggerOperations GenerateSchemaGetOperation(Schema schema, string schemaName, JsonSchema4 dataSchema)
        {
            return AddOperation(SwaggerOperationMethod.Get, schemaName, $"{appBasePath}/{schema.Name}/{{id}}", operation =>
            {
                operation.Summary = $"Get a {schemaName} content.";

                var responseSchema = CreateContentSchema(schemaName, schema.Name, dataSchema);

                operation.AddResponse("200", $"{schemaName} content found.", responseSchema);
            });
        }

        private SwaggerOperations GenerateSchemaCreateOperation(Schema schema, string schemaName, JsonSchema4 dataSchema)
        {
            return AddOperation(SwaggerOperationMethod.Post, null, $"{appBasePath}/{schema.Name}", operation =>
            {
                operation.Summary = $"Create a {schemaName} content.";

                operation.AddBodyParameter(dataSchema, "data", string.Format(BodyDescription, schemaName));

                operation.AddResponse("201", $"{schemaName} created.",  entityCreatedDtoSchema);
            });
        }

        private SwaggerOperations GenerateSchemaUpdateOperation(Schema schema, string schemaName, JsonSchema4 dataSchema)
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appBasePath}/{schema.Name}/{{id}}", operation =>
            {
                operation.Summary = $"Update a {schemaName} content.";

                operation.AddBodyParameter(dataSchema, "data", string.Format(BodyDescription, schemaName));

                operation.AddResponse("204", $"{schemaName} element updated.");
            });
        }

        private SwaggerOperations GenerateSchemaPatchOperation(Schema schema, string schemaName, JsonSchema4 dataSchema)
        {
            return AddOperation(SwaggerOperationMethod.Patch, schemaName, $"{appBasePath}/{schema.Name}/{{id}}", operation =>
            {
                operation.Summary = $"Patchs a {schemaName} content.";

                operation.AddBodyParameter(dataSchema, "data", string.Format(BodyDescription, schemaName));

                operation.AddResponse("204", $"{schemaName} element updated.");
            });
        }

        private SwaggerOperations GenerateSchemaPublishOperation(Schema schema, string schemaName)
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appBasePath}/{schema.Name}/{{id}}/publish", operation =>
            {
                operation.Summary = $"Publish a {schemaName} content.";
                
                operation.AddResponse("204", $"{schemaName} element published.");
            });
        }

        private SwaggerOperations GenerateSchemaUnpublishOperation(Schema schema, string schemaName)
        {
            return AddOperation(SwaggerOperationMethod.Put, schemaName, $"{appBasePath}/{schema.Name}/{{id}}/unpublish", operation =>
            {
                operation.Summary = $"Unpublish a {schemaName} content.";

                operation.AddResponse("204", $"{schemaName} element unpublished.");
            });
        }

        private SwaggerOperations GenerateSchemaDeleteOperation(Schema schema, string schemaName)
        {
            return AddOperation(SwaggerOperationMethod.Delete, schemaName, $"{appBasePath}/{schema.Name}/{{id}}/", operation =>
            {
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
                operation.AddPathParameter("id", JsonObjectType.String, $"The id of the {entityName} (GUID).");

                operation.AddResponse("404", $"App, schema or {entityName} content not found.");
            }

            return operations;
        }

        private JsonSchema4 CreateContentsSchema(string schemaName, string id, JsonSchema4 dataSchema)
        {
            var contentSchema = CreateContentSchema(schemaName, id, dataSchema);

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

        private JsonSchema4 CreateContentSchema(string schemaName, string id, JsonSchema4 dataSchema)
        {
            var CreateProperty = 
                new Func<string, string, JsonProperty>((d, f) => 
                    new JsonProperty { Description = d, Format = f, IsRequired = true, Type = JsonObjectType.String });

            var dataDescription = $"The data of the {schemaName} content.";
            var dataProperty = new JsonProperty { Description = dataDescription, Type = JsonObjectType.Object, IsRequired = true, SchemaReference = dataSchema };

            var schema = new JsonSchema4
            {
                Properties =
                {
                    ["id"] = CreateProperty($"The id of the {schemaName} content.", null),
                    ["data"] = dataProperty,
                    ["created"] = CreateProperty($"The date and time when the {schemaName} content has been created.", "date-time"),
                    ["createdBy"] = CreateProperty($"The user that has created the {schemaName} content.", null),
                    ["lastModified"] = CreateProperty($"The date and time when the {schemaName} content has been modified last.", "date-time"),
                    ["lastModifiedBy"] = CreateProperty($"The user that has updated the {schemaName} content last.", null)
                },
                Type = JsonObjectType.Object
            };

            return AppendSchema($"{id}ContentDto", schema);
        }

        private JsonSchema4 AppendSchema(string name, JsonSchema4 schema)
        {
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            return new JsonSchema4 { SchemaReference = document.Definitions.GetOrAdd(name, x => schema) };
        }
    }
}
