// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
using Squidex.Areas.Api.Config.Swagger;
using Squidex.Config;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemasSwaggerGenerator
    {
        private readonly MyUrlsOptions urlOptions;
        private readonly SwaggerDocumentSettings settings = new SwaggerDocumentSettings();
        private SwaggerJsonSchemaGenerator schemaGenerator;
        private SwaggerDocument document;
        private JsonSchemaResolver schemaResolver;

        public SchemasSwaggerGenerator(IOptions<MyUrlsOptions> urlOptions)
        {
            this.urlOptions = urlOptions.Value;

            settings.ConfigureNames();
            settings.Conf
        }

        public async Task<SwaggerDocument> Generate(HttpContext httpContext, IAppEntity app, IEnumerable<ISchemaEntity> schemas)
        {
            document = SwaggerHelper.CreateApiDocument(httpContext, urlOptions, app.Name);

            schemaGenerator = new SwaggerJsonSchemaGenerator(settings);
            schemaResolver = new SwaggerSchemaResolver(document, settings);

            GenerateSchemasOperations(schemas, app);

            await GenerateDefaultErrorsAsync();

            return document;
        }

        private void GenerateSchemasOperations(IEnumerable<ISchemaEntity> schemas, IAppEntity app)
        {
            var appBasePath = $"/content/{app.Name}";

            foreach (var schema in schemas.Where(x => x.IsPublished).Select(x => x.SchemaDef))
            {
                new SchemaSwaggerGenerator(document, appBasePath, schema, AppendSchema, app.PartitionResolver()).GenerateSchemaOperations();
            }
        }

        private async Task GenerateDefaultErrorsAsync()
        {
            const string errorDescription = "Operation failed with internal server error.";

            var errorDtoSchema = await schemaGenerator.GetErrorDtoSchemaAsync(schemaResolver);

            foreach (var operation in document.Paths.Values.SelectMany(x => x.Values))
            {
                operation.Responses.Add("500", new SwaggerResponse { Description = errorDescription, Schema = errorDtoSchema });
            }
        }

        private JsonSchema4 AppendSchema(string name, JsonSchema4 schema)
        {
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            return new JsonSchema4 { Reference = document.Definitions.GetOrAdd(name, schema, (k, c) => c) };
        }
    }
}
