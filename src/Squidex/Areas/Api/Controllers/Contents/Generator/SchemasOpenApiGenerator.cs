// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using NSwag.Generation;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.OpenApi;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemasOpenApiGenerator
    {
        private readonly UrlsOptions urlOptions;
        private readonly OpenApiDocumentGeneratorSettings settings = new OpenApiDocumentGeneratorSettings();
        private OpenApiSchemaGenerator schemaGenerator;
        private OpenApiDocument document;
        private JsonSchema statusSchema;
        private JsonSchemaResolver schemaResolver;

        public SchemasOpenApiGenerator(IOptions<UrlsOptions> urlOptions, IEnumerable<IDocumentProcessor> documentProcessors)
        {
            this.urlOptions = urlOptions.Value;

            settings.ConfigureSchemaSettings();

            foreach (var processor in documentProcessors)
            {
                settings.DocumentProcessors.Add(processor);
            }
        }

        public OpenApiDocument Generate(HttpContext httpContext, IAppEntity app, IEnumerable<ISchemaEntity> schemas)
        {
            document = NSwagHelper.CreateApiDocument(httpContext, urlOptions, app.Name);

            schemaGenerator = new OpenApiSchemaGenerator(settings);
            schemaResolver = new OpenApiSchemaResolver(document, settings);

            statusSchema = GenerateStatusSchema();

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
                processor.Process(context);
            }

            return document;
        }

        private JsonSchema GenerateStatusSchema()
        {
            var statusDtoType = typeof(ChangeStatusDto);

            return schemaGenerator.Generate(statusDtoType);
        }

        private void GenerateSchemasOperations(IEnumerable<ISchemaEntity> schemas, IAppEntity app)
        {
            var appBasePath = $"/content/{app.Name}";

            foreach (var schema in schemas.Select(x => x.SchemaDef).Where(x => x.IsPublished))
            {
                var partition = app.PartitionResolver();

                new SchemaOpenApiGenerator(document, app.Name, appBasePath, schema, AppendSchema, statusSchema, partition)
                    .GenerateSchemaOperations();
            }
        }

        private JsonSchema AppendSchema(string name, JsonSchema schema)
        {
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            return new JsonSchema { Reference = document.Definitions.GetOrAdd(name, schema, (k, c) => c) };
        }
    }
}
