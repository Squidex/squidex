// ==========================================================================
//  SchemasSwaggerGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration;
using Squidex.Config;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;

namespace Squidex.Controllers.ContentApi.Generator
{
    public sealed class SchemasSwaggerGenerator
    {
        private readonly HttpContext context;
        private readonly SwaggerSettings settings;
        private readonly MyUrlsOptions urlOptions;
        private SwaggerJsonSchemaGenerator schemaGenerator;
        private JsonSchemaResolver schemaResolver;
        private SwaggerGenerator swaggerGenerator;
        private SwaggerDocument document;

        public SchemasSwaggerGenerator(IHttpContextAccessor context, SwaggerSettings settings, IOptions<MyUrlsOptions> urlOptions)
        {
            this.context = context.HttpContext;
            this.settings = settings;
            this.urlOptions = urlOptions.Value;
        }

        public async Task<SwaggerDocument> Generate(IAppEntity app, IEnumerable<ISchemaEntity> schemas)
        {
            document = SwaggerHelper.CreateApiDocument(context, urlOptions, app.Name);

            schemaGenerator = new SwaggerJsonSchemaGenerator(settings);
            schemaResolver = new SwaggerSchemaResolver(document, settings);

            swaggerGenerator = new SwaggerGenerator(schemaGenerator, settings, schemaResolver);

            GenerateSchemasOperations(schemas, app);

            await GenerateDefaultErrorsAsync();

            return document;
        }

        private void GenerateSchemasOperations(IEnumerable<ISchemaEntity> schemas, IAppEntity app)
        {
            var appBasePath = $"/content/{app.Name}";

            foreach (var schema in schemas.Where(x => x.IsPublished).Select(x => x.SchemaDef))
            {
                new SchemaSwaggerGenerator(document, appBasePath, schema, AppendSchema, app.PartitionResolver).GenerateSchemaOperations();
            }
        }

        private async Task GenerateDefaultErrorsAsync()
        {
            const string errorDescription = "Operation failed with internal server error.";

            var errorDtoSchema = await swaggerGenerator.GetErrorDtoSchemaAsync();

            foreach (var operation in document.Paths.Values.SelectMany(x => x.Values))
            {
                operation.Responses.Add("500", new SwaggerResponse { Description = errorDescription, Schema = errorDtoSchema });
            }
        }

        private JsonSchema4 AppendSchema(string name, JsonSchema4 schema)
        {
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            return new JsonSchema4 { SchemaReference = document.Definitions.GetOrAdd(name, x => schema) };
        }
    }
}
