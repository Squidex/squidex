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
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Caching;
using Squidex.Pipeline.OpenApi;
using Squidex.Shared;

namespace Squidex.Areas.Api.Controllers.Contents.Generator
{
    public sealed class SchemasOpenApiGenerator
    {
        private readonly OpenApiDocumentGeneratorSettings settings = new OpenApiDocumentGeneratorSettings();
        private readonly OpenApiSchemaGenerator schemaGenerator;
        private readonly IRequestCache requestCache;

        public SchemasOpenApiGenerator(IEnumerable<IDocumentProcessor> documentProcessors, IRequestCache requestCache)
        {
            settings.ConfigureSchemaSettings();

            foreach (var processor in documentProcessors)
            {
                settings.DocumentProcessors.Add(processor);
            }

            schemaGenerator = new OpenApiSchemaGenerator(settings);

            this.requestCache = requestCache;
        }

        public OpenApiDocument Generate(HttpContext httpContext, IAppEntity app, IEnumerable<ISchemaEntity> schemas, bool flat = false)
        {
            var document = OpenApiHelper.CreateApiDocument(httpContext, app.Name);

            var schemaResolver = new OpenApiSchemaResolver(document, settings);

            requestCache.AddDependency(app.UniqueId, app.Version);

            var builder = new AppBuilder(
                app, 
                document,
                schemaResolver,
                schemaGenerator,
                flat);

            foreach (var schema in schemas.Where(x => x.SchemaDef.IsPublished))
            {
                requestCache.AddDependency(schema.UniqueId, schema.Version);

                GenerateSchemaOperations(builder.Schema(schema.SchemaDef));
            }

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

        private static void GenerateSchemaOperations(OperationsBuilder builder)
        {
            var contentsSchema = new JsonSchema
            {
                Properties =
                {
                    ["total"] = SchemaBuilder.NumberProperty(builder.FormatText("The total number of schema content items."), true),
                    ["items"] = SchemaBuilder.ArrayProperty(builder.ContentSchema, builder.FormatText("The schema content items."), true)
                },
                Type = JsonObjectType.Object
            };

            builder.AddOperation(OpenApiOperationMethod.Get, "/")
                .RequirePermission(Permissions.AppContentsReadOwn)
                .Operation("Query")
                .OperationSummary("Query schema contents items.")
                .Describe(OpenApiHelper.SchemaQueryDocs)
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
                .HasId()
                .HasBody("data", builder.DataSchema, OpenApiHelper.SchemaBodyDocs)
                .Responds(201, "Content item created", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Post, "/{id}")
                .RequirePermission(Permissions.AppContentsUpsert)
                .Operation("Upsert")
                .OperationSummary("Upsert a schema content item.")
                .HasQuery("publish", JsonObjectType.Boolean, "True to automatically publish the content.")
                .HasId()
                .HasBody("data", builder.DataSchema, OpenApiHelper.SchemaBodyDocs)
                .Responds(200, "Content item created or updated.", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Put, "/{id}")
                .RequirePermission(Permissions.AppContentsUpdateOwn)
                .Operation("Update")
                .OperationSummary("Update a schema content item.")
                .HasId()
                .HasBody("data", builder.DataSchema, OpenApiHelper.SchemaBodyDocs)
                .Responds(200, "Content item updated.", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Patch, "/{id}")
                .RequirePermission(Permissions.AppContentsUpdateOwn)
                .Operation("Patch")
                .OperationSummary("Patch a schema content item.")
                .HasId()
                .HasBody("data", builder.DataSchema, OpenApiHelper.SchemaBodyDocs)
                .Responds(200, "Content item updated.", builder.ContentSchema)
                .Responds(400, "Content data not valid.");

            builder.AddOperation(OpenApiOperationMethod.Put, "/{id}/status")
                .RequirePermission(Permissions.AppContentsUpdateOwn)
                .Operation("Patch")
                .OperationSummary("Patch a schema content item.")
                .HasId()
                .HasBody("data", builder.DataSchema, OpenApiHelper.SchemaBodyDocs)
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
    }
}
