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
using Squidex.Domain.Apps.Read.Contents.CustomQueries;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;

#pragma warning disable IDE0017 // Simplify object initialization

namespace Squidex.Controllers.ContentApi.Generator
{
    public sealed class SchemasSwaggerGenerator
    {
        private readonly HttpContext context;
        private readonly SwaggerSettings settings;
        private readonly MyUrlsOptions urlOptions;
        private SwaggerDocument document;
        private SwaggerJsonSchemaGenerator schemaGenerator;
        private JsonSchemaResolver schemaResolver;

        public SchemasSwaggerGenerator(IHttpContextAccessor context, SwaggerSettings settings, IOptions<MyUrlsOptions> urlOptions)
        {
            this.context = context.HttpContext;
            this.settings = settings;
            this.urlOptions = urlOptions.Value;
        }

        public async Task<SwaggerDocument> GenerateAsync(
            IAppEntity app,
            IEnumerable<ISchemaEntity> schemas,
            IEnumerable<ICustomQuery> queries)
        {
            document = SwaggerHelper.CreateApiDocument(context, urlOptions, app.Name);

            schemaGenerator = new SwaggerJsonSchemaGenerator(settings);
            schemaResolver = new SwaggerSchemaResolver(document, settings);

            var path = $"/content/{app.Name}";

            foreach (var schema in schemas)
            {
                GenerateSchemaOperations(app, path, schema);
            }

            foreach (var query in queries)
            {
                GenerateQueryOperation(query, path);
            }

            await GenerateDefaultErrorsAsync();

            return document;
        }

        private void GenerateSchemaOperations(IAppEntity app, string path, ISchemaEntity schema)
        {
            var generator = new SchemaSwaggerGenerator(document, path, schema.SchemaDef, AppendSchema, app.PartitionResolver);

            generator.GenerateSchemaOperations();
        }

        public void GenerateQueryOperation(ICustomQuery query, string path)
        {
            var operation = new SwaggerOperation();

            operation.OperationId = $"CustomQuery{query.Name}Contents";
            operation.Summary = query.Summary;
            operation.Security = Definitions.ReaderSecurity;
            operation.Description = query.Description;

            operation.Tags = new List<string> { "CustomQueries" };

            foreach (var argument in query.ArgumentOptions)
            {
                operation.AddQueryParameter(argument.Name, JsonObjectType.String, argument.Description);
            }

            operation.AddResponse("200", $"{query.Name} content retrieved.");

            var queryPath = $"{path}/queries/{query.Name}";

            document.Paths[queryPath] = new SwaggerOperations
            {
                [SwaggerOperationMethod.Get] = operation
            };
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

            return new JsonSchema4 { Reference = document.Definitions.GetOrAdd(name, x => schema) };
        }
    }
}