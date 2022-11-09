// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag;
using NSwag.Generation;
using NSwag.Generation.Processors.Contexts;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Caching;
using Squidex.Properties;
using Squidex.Shared;
using IRequestUrlGenerator = Squidex.Hosting.IUrlGenerator;
using SchemaDefType = Squidex.Domain.Apps.Core.Schemas.SchemaType;

namespace Squidex.Areas.Api.Controllers.Contents.Generator;

public sealed class SchemasOpenApiGenerator
{
    private readonly IAppProvider appProvider;
    private readonly OpenApiDocumentGeneratorSettings schemaSettings;
    private readonly OpenApiSchemaGenerator schemaGenerator;
    private readonly IRequestUrlGenerator urlGenerator;
    private readonly IRequestCache requestCache;

    public SchemasOpenApiGenerator(
        IAppProvider appProvider,
        OpenApiDocumentGeneratorSettings schemaSettings,
        OpenApiSchemaGenerator schemaGenerator,
        IRequestUrlGenerator urlGenerator,
        IRequestCache requestCache)
    {
        this.appProvider = appProvider;
        this.urlGenerator = urlGenerator;
        this.schemaSettings = schemaSettings;
        this.schemaGenerator = schemaGenerator;
        this.requestCache = requestCache;
    }

    public async Task<OpenApiDocument> GenerateAsync(HttpContext httpContext, IAppEntity app, IEnumerable<ISchemaEntity> schemas, bool flat)
    {
        var document = CreateApiDocument(httpContext, app);

        var schemaResolver = new OpenApiSchemaResolver(document, schemaSettings);

        requestCache.AddDependency(app.UniqueId, app.Version);

        foreach (var schema in schemas)
        {
            requestCache.AddDependency(schema.UniqueId, schema.Version);
        }

        var builder = new Builder(app, document, schemaResolver, schemaGenerator);

        var validSchemas =
            schemas.Where(x =>
                x.SchemaDef.IsPublished &&
                x.SchemaDef.Type != SchemaDefType.Component &&
                x.SchemaDef.Fields.Count > 0);

        var partitionResolver = app.PartitionResolver();

        foreach (var schema in validSchemas)
        {
            var components = await appProvider.GetComponentsAsync(schema, httpContext.RequestAborted);

            GenerateSchemaOperations(builder.Schema(schema.SchemaDef, partitionResolver, components, flat));
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
        builder.AddOperation(OpenApiOperationMethod.Get, "/")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Operation("Query")
            .OperationSummary("Query contents across all schemas.")
            .HasQuery("ids", JsonObjectType.String, "Comma-separated list of content IDs.")
            .Responds(200, "Content items retrieved.", builder.ContentsSchema)
            .Responds(400, "Query not valid.");

        builder.AddOperation(OpenApiOperationMethod.Post, "/bulk")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Operation("Bulk")
            .OperationSummary("Bulk update content items across all schemas.")
            .HasBody("request", builder.Parent.BulkRequestSchema, null)
            .Responds(200, "Contents created, update or delete.", builder.Parent.BulkResponseSchema)
            .Responds(400, "Contents request not valid.");
    }

    private static void GenerateSchemaOperations(OperationsBuilder builder)
    {
        builder.AddOperation(OpenApiOperationMethod.Get, "/")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Operation("Query")
            .OperationSummary("Query [schema] contents items.")
            .Describe(Resources.OpenApiSchemaQuery)
            .HasQueryOptions(true)
            .Responds(200, "Content items retrieved.", builder.ContentsSchema)
            .Responds(400, "Content query not valid.");

        builder.AddOperation(OpenApiOperationMethod.Post, "/query")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Operation("QueryPost")
            .OperationSummary("Query [schema] contents items using Post.")
            .HasBody("query", builder.Parent.QuerySchema, null)
            .Responds(200, "Content items retrieved.", builder.ContentsSchema)
            .Responds(400, "Content query not valid.");

        builder.AddOperation(OpenApiOperationMethod.Get, "/{id}")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Operation("Get")
            .OperationSummary("Get a [schema] content item.")
            .HasQuery("version", JsonObjectType.Number, FieldDescriptions.EntityVersion)
            .HasId()
            .Responds(200, "Content item returned.", builder.ContentSchema);

        builder.AddOperation(OpenApiOperationMethod.Get, "/{id}/{version}")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Deprecated()
            .Operation("GetVersioned")
            .OperationSummary("Get a [schema] content item by id and version.")
            .HasPath("version", JsonObjectType.Number, FieldDescriptions.EntityVersion)
            .HasId()
            .Responds(200, "Content item returned.", builder.DataSchema);

        builder.AddOperation(OpenApiOperationMethod.Get, "/{id}/validity")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Operation("Validate")
            .OperationSummary("Validates a [schema] content item.")
            .HasId()
            .Responds(200, "Content item is valid.")
            .Responds(400, "Content item is not valid.");

        builder.AddOperation(OpenApiOperationMethod.Post, "/")
            .RequirePermission(PermissionIds.AppContentsCreate)
            .Operation("Create")
            .OperationSummary("Create a [schema] content item.")
            .HasQuery("publish", JsonObjectType.Boolean, FieldDescriptions.ContentRequestPublish)
            .HasQuery("id", JsonObjectType.String, FieldDescriptions.ContentRequestOptionalId)
            .HasBody("data", builder.DataSchema, Resources.OpenApiSchemaBody)
            .Responds(201, "Content item created", builder.ContentSchema)
            .Responds(400, "Content data not valid.");

        builder.AddOperation(OpenApiOperationMethod.Post, "/bulk")
            .RequirePermission(PermissionIds.AppContentsReadOwn)
            .Operation("Bulk")
            .OperationSummary("Bulk update content items.")
            .HasBody("request", builder.Parent.BulkRequestSchema, null)
            .Responds(200, "Contents created, update or delete.", builder.Parent.BulkResponseSchema)
            .Responds(400, "Contents request not valid.");

        builder.AddOperation(OpenApiOperationMethod.Post, "/{id}")
            .RequirePermission(PermissionIds.AppContentsUpsert)
            .Operation("Upsert")
            .OperationSummary("Upsert a [schema] content item.")
            .HasQuery("patch", JsonObjectType.Boolean, FieldDescriptions.ContentRequestPatch)
            .HasQuery("publish", JsonObjectType.Boolean, FieldDescriptions.ContentRequestPublish)
            .HasId()
            .HasBody("data", builder.DataSchema, Resources.OpenApiSchemaBody)
            .Responds(200, "Content item created or updated.", builder.ContentSchema)
            .Responds(400, "Content data not valid.");

        builder.AddOperation(OpenApiOperationMethod.Put, "/{id}")
            .RequirePermission(PermissionIds.AppContentsUpdateOwn)
            .Operation("Update")
            .OperationSummary("Update a [schema] content item.")
            .HasId()
            .HasBody("data", builder.DataSchema, Resources.OpenApiSchemaBody)
            .Responds(200, "Content item updated.", builder.ContentSchema)
            .Responds(400, "Content data not valid.");

        builder.AddOperation(OpenApiOperationMethod.Patch, "/{id}")
            .RequirePermission(PermissionIds.AppContentsUpdateOwn)
            .Operation("Patch")
            .OperationSummary("Patch a [schema] content item.")
            .HasId()
            .HasBody("data", builder.DataSchema, Resources.OpenApiSchemaBody)
            .Responds(200, "Content item updated.", builder.ContentSchema)
            .Responds(400, "Content data not valid.");

        builder.AddOperation(OpenApiOperationMethod.Put, "/{id}/status")
            .RequirePermission(PermissionIds.AppContentsChangeStatusOwn)
            .Operation("Change")
            .OperationSummary("Change the status of a [schema] content item.")
            .HasId()
            .HasBody("request", builder.Parent.ChangeStatusSchema, "The request to change content status.")
            .Responds(200, "Content status updated.", builder.ContentSchema)
            .Responds(400, "Content status not valid.");

        builder.AddOperation(OpenApiOperationMethod.Delete, "/{id}")
            .RequirePermission(PermissionIds.AppContentsDeleteOwn)
            .Operation("Delete")
            .OperationSummary("Delete a [schema] content item.")
            .HasQuery("permanent", JsonObjectType.Boolean, FieldDescriptions.EntityRequestDeletePermanent)
            .HasId()
            .Responds(204, "Content item deleted");
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
                    Resources.OpenApiContentDescription
                        .Replace("[REDOC_LINK_NORMAL]", urlGenerator.BuildUrl($"api/content/{app.Name}/docs"), StringComparison.Ordinal)
                        .Replace("[REDOC_LINK_SIMPLE]", urlGenerator.BuildUrl($"api/content/{app.Name}/docs/flat"), StringComparison.Ordinal)
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
