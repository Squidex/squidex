﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NJsonSchema;
using NSwag;
using NSwag.Generation;
using NSwag.Generation.Processors.Contexts;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Hosting;
using Squidex.Infrastructure.Caching;
using Squidex.Shared;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemasOpenApiGenerator
    {
        private readonly IUrlGenerator urlGenerator;
        private readonly OpenApiDocumentGeneratorSettings schemaSettings;
        private readonly OpenApiSchemaGenerator schemaGenerator;
        private readonly IRequestCache requestCache;

        public SchemasOpenApiGenerator(
            IUrlGenerator urlGenerator,
            OpenApiDocumentGeneratorSettings schemaSettings,
            OpenApiSchemaGenerator schemaGenerator,
            IRequestCache requestCache)
        {
            this.urlGenerator = urlGenerator;
            this.schemaSettings = schemaSettings;
            this.schemaGenerator = schemaGenerator;
            this.requestCache = requestCache;
        }

        public OpenApiDocument Generate(HttpContext httpContext, IAppEntity app, IEnumerable<ISchemaEntity> schemas, bool flat = false)
        {
            var document = CreateApiDocument(httpContext, app);

            var schemaResolver = new OpenApiSchemaResolver(document, schemaSettings);

            requestCache.AddDependency(app.UniqueId, app.Version);

            var builder = new Builder(
                app,
                document,
                schemaResolver,
                schemaGenerator);

            foreach (var schema in schemas.Where(x => x.SchemaDef.IsPublished))
            {
                requestCache.AddDependency(schema.UniqueId, schema.Version);

                GenerateSchemaOperations(builder.Schema(schema.SchemaDef, flat));
            }

            GenerateSharedOperations(builder.Shared());

            var context =
                new DocumentProcessorContext(document,
                    Enumerable.Empty<Type>(),
                    Enumerable.Empty<Type>(),
                    schemaResolver,
                    schemaGenerator,
                    schemaSettings);

            foreach (var processor in schemaSettings.DocumentProcessors)
            {
                processor.Process(context);
            }

            return document;
        }

        private static void GenerateSharedOperations(OperationsBuilder builder)
        {
            var contentsSchema = BuildResults(builder);

            builder.AddOperation(OpenApiOperationMethod.Get, "/")
                .RequirePermission(Permissions.AppContentsReadOwn)
                .Operation("Query")
                .OperationSummary("Query contents across all schemas.")
                .HasQuery("ids", JsonObjectType.String, "Comma-separated list of content IDs.")
                .Responds(200, "Content items retrieved.", contentsSchema)
                .Responds(400, "Query not valid.");
        }

        private static void GenerateSchemaOperations(OperationsBuilder builder)
        {
            var contentsSchema = BuildResults(builder);

            builder.AddOperation(OpenApiOperationMethod.Get, "/")
                .RequirePermission(Permissions.AppContentsReadOwn)
                .Operation("Query")
                .OperationSummary("Query schema contents items.")
                .Describe(Properties.Resources.OpenApiSchemaQuery)
                .HasQueryOptions(true)
                .Responds(200, "Content items retrieved.", contentsSchema)
                .Responds(400, "Query not valid.");

            builder.AddOperation(OpenApiOperationMethod.Get, "/{id}")
                .RequirePermission(Permissions.AppContentsReadOwn)
                .Operation("Get")
                .OperationSummary("Get a schema content item.")
                .HasId()
                .Responds(200, "Content item returned.", builder.ContentSchema);

            builder.AddOperation(OpenApiOperationMethod.Get, "/{id}/{version}")
                .RequirePermission(Permissions.AppContentsReadOwn)
                .Operation("Get")
                .OperationSummary("Get a schema content item by id and version.")
                .HasPath("version", JsonObjectType.Number, "The version of the content item.")
                .HasId()
                .Responds(200, "Content item returned.", builder.ContentSchema);

            builder.AddOperation(OpenApiOperationMethod.Get, "/{id}/validity")
                .RequirePermission(Permissions.AppContentsReadOwn)
                .Operation("Validate")
                .OperationSummary("Validates a schema content item.")
                .HasId()
                .Responds(200, "Content item is valid.")
                .Responds(400, "Content item is not valid.");

            builder.AddOperation(OpenApiOperationMethod.Post, "/")
                .RequirePermission(Permissions.AppContentsCreate)
                .Operation("Create")
                .OperationSummary("Create a schema content item.")
                .HasQuery("publish", JsonObjectType.Boolean, "True to automatically publish the content.")
                .HasQuery("id", JsonObjectType.String, "The optional custom content id.")
                .HasBody("data", builder.DataSchema, Properties.Resources.OpenApiSchemaBody)
                .Responds(201, "Content item created", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Post, "/{id}")
                .RequirePermission(Permissions.AppContentsUpsert)
                .Operation("Upsert")
                .OperationSummary("Upsert a schema content item.")
                .HasQuery("publish", JsonObjectType.Boolean, "True to automatically publish the content.")
                .HasId()
                .HasBody("data", builder.DataSchema, Properties.Resources.OpenApiSchemaBody)
                .Responds(200, "Content item created or updated.", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Put, "/{id}")
                .RequirePermission(Permissions.AppContentsUpdateOwn)
                .Operation("Update")
                .OperationSummary("Update a schema content item.")
                .HasId()
                .HasBody("data", builder.DataSchema, Properties.Resources.OpenApiSchemaBody)
                .Responds(200, "Content item updated.", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Patch, "/{id}")
                .RequirePermission(Permissions.AppContentsUpdateOwn)
                .Operation("Patch")
                .OperationSummary("Patch a schema content item.")
                .HasId()
                .HasBody("data", builder.DataSchema, Properties.Resources.OpenApiSchemaBody)
                .Responds(200, "Content item updated.", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Put, "/{id}/status")
                .RequirePermission(Permissions.AppContentsUpdateOwn)
                .Operation("Patch")
                .OperationSummary("Patch a schema content item.")
                .HasId()
                .HasBody("data", builder.DataSchema, Properties.Resources.OpenApiSchemaBody)
                .Responds(200, "Content item updated.", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Delete, "/{id}")
                .RequirePermission(Permissions.AppContentsChangeStatusOwn)
                .Operation("Change")
                .OperationSummary("Change the status of a schema content item.")
                .HasId()
                .HasBody("request", builder.Parent.ChangeStatusSchema, "The request to change content status.")
                .Responds(200, "Content status updated.", builder.ContentSchema);

            builder.AddOperation(OpenApiOperationMethod.Delete, "/{id}")
                .RequirePermission(Permissions.AppContentsDeleteOwn)
                .Operation("Delete")
                .OperationSummary("Delete a schema content item.")
                .HasId()
                .Responds(204, "Content item deleted");
        }

        private static JsonSchema BuildResults(OperationsBuilder builder)
        {
            return new JsonSchema
            {
                Properties =
                {
                    ["total"] = SchemaBuilder.NumberProperty("The total number of content items.", true),
                    ["items"] = SchemaBuilder.ArrayProperty(builder.ContentSchema, "The content items.", true)
                },
                Type = JsonObjectType.Object
            };
        }

        private OpenApiDocument CreateApiDocument(HttpContext context, IAppEntity app)
        {
            var appName = app.Name;

            var scheme =
                string.Equals(context.Request.Scheme, "http", StringComparison.OrdinalIgnoreCase) ?
                    OpenApiSchema.Http :
                    OpenApiSchema.Https;

            var document = new OpenApiDocument
            {
                Schemes = new List<OpenApiSchema>
                {
                    scheme
                },
                Consumes = new List<string>
                {
                    "application/json"
                },
                Produces = new List<string>
                {
                    "application/json"
                },
                Info = new OpenApiInfo
                {
                    Title = $"Squidex Content API for '{appName}' App",
                    Description =
                        Properties.Resources.OpenApiContentDescription
                            .Replace("[REDOC_LINK_NORMAL]", urlGenerator.BuildUrl($"api/content/{app.Name}/docs"))
                            .Replace("[REDOC_LINK_SIMPLE]", urlGenerator.BuildUrl($"api/content/{app.Name}/docs/flat"))
                },
                SchemaType = SchemaType.OpenApi3
            };

            if (!string.IsNullOrWhiteSpace(context.Request.Host.Value))
            {
                document.Host = context.Request.Host.Value;
            }

            return document;
        }
    }
}
