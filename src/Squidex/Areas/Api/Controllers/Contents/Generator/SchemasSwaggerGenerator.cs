// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Areas.Api.Config.Swagger;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemasSwaggerGenerator
    {
        private readonly UrlsOptions urlOptions;
        private readonly SwaggerDocumentSettings settings = new SwaggerDocumentSettings();
        private SwaggerJsonSchemaGenerator schemaGenerator;
        private SwaggerDocument document;
        private JsonSchema4 statusSchema;
        private JsonSchemaResolver schemaResolver;

        public SchemasSwaggerGenerator(IOptions<UrlsOptions> urlOptions, IEnumerable<IDocumentProcessor> documentProcessors)
        {
            this.urlOptions = urlOptions.Value;

            settings.ConfigureSchemaSettings();

            foreach (var processor in documentProcessors)
            {
                settings.DocumentProcessors.Add(processor);
            }
        }

        public async Task<SwaggerDocument> Generate(HttpContext httpContext, IAppEntity app, IEnumerable<ISchemaEntity> schemas)
        {
            document = NSwagHelper.CreateApiDocument(httpContext, urlOptions, app.Name);

            schemaGenerator = new SwaggerJsonSchemaGenerator(settings);
            schemaResolver = new SwaggerSchemaResolver(document, settings);

            statusSchema = await GenerateStatusSchemaAsync();

            GenerateSchemasOperations(schemas, app);

            var context =
                new DocumentProcessorContext(document,
                    Enumerable.Empty<Type>(),
                    Enumerable.Empty<Type>(),
                    schemaResolver,
                    schemaGenerator,
                    settings);

            foreach (var processor in settings.DocumentProcessors)
            {
                await processor.ProcessAsync(context);
            }

            return document;
        }

        private Task<JsonSchema4> GenerateStatusSchemaAsync()
        {
            var errorType = typeof(ChangeStatusDto);

            return schemaGenerator.GenerateWithReferenceAsync<JsonSchema4>(errorType, Enumerable.Empty<Attribute>(), schemaResolver);
        }

        private void GenerateSchemasOperations(IEnumerable<ISchemaEntity> schemas, IAppEntity app)
        {
            var appBasePath = $"/content/{app.Name}";

            foreach (var schema in schemas.Select(x => x.SchemaDef).Where(x => x.IsPublished))
            {
                var partition = app.PartitionResolver();

                new SchemaSwaggerGenerator(document, app.Name, appBasePath, schema, AppendSchema, statusSchema, partition)
                    .GenerateSchemaOperations();
            }
        }

        private JsonSchema4 AppendSchema(string name, JsonSchema4 schema)
        {
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            return new JsonSchema4 { Reference = document.Definitions.GetOrAdd(name, schema, (k, c) => c) };
        }
    }
}
