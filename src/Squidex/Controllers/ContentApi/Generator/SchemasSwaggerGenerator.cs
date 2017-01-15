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
using NSwag.CodeGeneration.SwaggerGenerators;
using Squidex.Config;
using Squidex.Controllers.Api;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;
using Squidex.Read.Apps;
using Squidex.Read.Schemas.Repositories;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Squidex.Controllers.ContentApi.Generator
{
    public sealed class SchemasSwaggerGenerator
    {
        private const string BodyDescription =
            @"The data of the {0} to be created or updated.
            
Please not that each field is an object with one entry per language. 
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

        public async Task<SwaggerDocument> Generate(IAppEntity appEntity, IReadOnlyCollection<ISchemaEntityWithSchema> schemas)
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
                    { "roles", new List<string> { "app-owner", "app-developer", "app-editor" } }
                }
            };

            foreach (var operation in document.Paths.Values.SelectMany(x => x.Values))
            {
                operation.Security = securityRequirements;
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
                    Name = schemaName, Description = $"API to managed {schemaName} content elements."
                });

            var noIdItemOperations =
                document.Paths.GetOrAdd($"{appBasePath}/{schema.Name}/", k => new SwaggerOperations());

            var idItemOperations =
                document.Paths.GetOrAdd($"{appBasePath}/{schema.Name}/{{id}}/", k => new SwaggerOperations());

            GenerateSchemaQueryOperation(noIdItemOperations, schema, schemaName);
            GenerateSchemaCreateOperation(noIdItemOperations, schema, schemaName);

            GenerateSchemaGetOperation(idItemOperations, schema, schemaName);
            GenerateSchemaUpdateOperation(idItemOperations, schema, schemaName);
            GenerateSchemaDeleteOperation(idItemOperations, schemaName);

            foreach (var operation in idItemOperations.Values.Union(noIdItemOperations.Values))
            {
                operation.Tags = new List<string> { schemaName };
            }

            foreach (var operation in idItemOperations.Values)
            {
                operation.Responses.Add("404",
                    new SwaggerResponse { Description = $"App, schema or {schemaName} not found." });

                operation.Parameters.AddPathParameter("id", JsonObjectType.String, $"The id of the {schemaName} (GUID).");
            }
        }

        private void GenerateSchemaQueryOperation(SwaggerOperations operations, Schema schema, string schemaName)
        {
            var operation = new SwaggerOperation
            {
                Summary = $"Queries {schemaName} content elements."
            };

            operation.Parameters.AddQueryParameter("take", JsonObjectType.Number, "The number of elements to take.");
            operation.Parameters.AddQueryParameter("skip", JsonObjectType.Number, "The number of elements to skip.");

            operation.Parameters.AddQueryParameter("query", JsonObjectType.String, "Optional full text query skip.");

            var responseSchema = CreateContentsSchema(schema.BuildSchema(languages, AppendSchema), schemaName, schema.Name);

            operation.Responses.Add("200",
                new SwaggerResponse { Description = $"{schemaName} content elements retrieved.", Schema = responseSchema });
            operation.Responses.Add("500",
                new SwaggerResponse { Description = $"Querying {schemaName} element failed with internal server error.", Schema = errorDtoSchema });

            operations[SwaggerOperationMethod.Get] = operation;
        }

        private void GenerateSchemaCreateOperation(SwaggerOperations operations, Schema schema, string schemaName)
        {
            var operation = new SwaggerOperation
            {
                Summary = $"Create a {schemaName} content element."
            };

            var bodySchema = AppendSchema($"{schema.Name}Dto", schema.BuildSchema(languages, AppendSchema));

            operation.Parameters.AddBodyParameter(bodySchema, "data", string.Format(BodyDescription, schemaName));

            operation.Responses.Add("201",
                new SwaggerResponse { Description = $"{schemaName} created.", Schema = entityCreatedDtoSchema });
            operation.Responses.Add("500",
                new SwaggerResponse { Description = $"Creating {schemaName} element failed with internal server error.", Schema = errorDtoSchema });

            operations[SwaggerOperationMethod.Post] = operation;
        }

        private void GenerateSchemaGetOperation(SwaggerOperations operations, Schema schema, string schemaName)
        {
            var operation = new SwaggerOperation
            {
                Summary = $"Gets a {schemaName} content element"
            };

            var responseSchema = CreateContentSchema(schema.BuildSchema(languages, AppendSchema), schemaName, schema.Name);

            operation.Responses.Add("209",
                new SwaggerResponse { Description = $"{schemaName} element found.", Schema = responseSchema });
            operation.Responses.Add("500",
                new SwaggerResponse { Description = $"Retrieving {schemaName} element failed with internal server error.", Schema = errorDtoSchema });

            operations[SwaggerOperationMethod.Get] = operation;
        }

        private void GenerateSchemaUpdateOperation(SwaggerOperations operations, Schema schema, string schemaName)
        {
            var operation = new SwaggerOperation
            {
                Summary = $"Update {schemaName} content element."
            };

            var bodySchema = AppendSchema($"{schema.Name}Dto", schema.BuildSchema(languages, AppendSchema));

            operation.Parameters.AddBodyParameter(bodySchema, "data", string.Format(BodyDescription, schemaName));

            operation.Responses.Add("204",
                new SwaggerResponse { Description = $"{schemaName} element updated." });
            operation.Responses.Add("500",
                new SwaggerResponse { Description = $"Updating {schemaName} element failed with internal server error.", Schema = errorDtoSchema });

            operations[SwaggerOperationMethod.Put] = operation;
        }

        private void GenerateSchemaDeleteOperation(SwaggerOperations operations, string schemaName)
        {
            var operation = new SwaggerOperation
            {
                Summary = $"Delete a {schemaName} content element."
            };
            
            operation.Responses.Add("204",
                new SwaggerResponse { Description = $"{schemaName} element deleted." });
            operation.Responses.Add("500",
                new SwaggerResponse { Description = $"Deleting {schemaName} element failed with internal server error.", Schema = errorDtoSchema });

            operations[SwaggerOperationMethod.Delete] = operation;
        }

        private JsonSchema4 CreateContentsSchema(JsonSchema4 dataSchema, string schemaName, string id)
        {
            var contentSchema = CreateContentSchema(dataSchema, schemaName, id);

            var schema = new JsonSchema4
            {
                Properties =
                {
                    ["total"] = new JsonProperty
                    {
                        Type = JsonObjectType.Number, IsRequired = true, Description = $"The total number of {schemaName} content elements."
                    },
                    ["items"] = new JsonProperty
                    {
                        IsRequired = true,
                        Item = contentSchema,
                        Type = JsonObjectType.Array,
                        Description = $"The item of {schemaName} content elements."
                    }
                },
                Type = JsonObjectType.Object
            };

            return schema;
        }

        private JsonSchema4 CreateContentSchema(JsonSchema4 dataSchema, string schemaName, string id)
        {
            var CreateProperty = 
                new Func<string, string, JsonProperty>((d, f) => 
                    new JsonProperty { Description = d, Format = f, IsRequired = true, Type = JsonObjectType.String });

            var dataProperty = new JsonProperty { Type = JsonObjectType.Object, IsRequired = true, Description = "The data of the content element" };

            dataProperty.AllOf.Add(dataSchema);

            var schema = new JsonSchema4
            {
                Properties =
                {
                    ["id"] = CreateProperty($"The id of the {schemaName}", null),
                    ["data"] = dataProperty,
                    ["created"] = CreateProperty($"The date and time when the {schemaName} content element has been created.", "date-time"),
                    ["createdBy"] = CreateProperty($"The user that has created the {schemaName} content element.", null),
                    ["lastModified"] = CreateProperty($"The date and time when the {schemaName} content element has been modified last.", "date-time"),
                    ["lastModifiedBy"] = CreateProperty($"The user that has updated the {schemaName} content element.", null),
                    ["isPublished"] = new JsonProperty
                    {
                        Description = $"Indicates if the {schemaName} content element is publihed.", IsRequired = true, Type = JsonObjectType.Boolean
                    }   
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
