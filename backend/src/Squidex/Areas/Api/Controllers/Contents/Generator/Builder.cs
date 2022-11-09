// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Namotion.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation;
using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.GenerateJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Contents.Generator;

internal sealed class Builder
{
    public string AppName { get; }

    public JsonSchema ChangeStatusSchema { get; }

    public JsonSchema BulkResponseSchema { get; }

    public JsonSchema BulkRequestSchema { get; }

    public JsonSchema QuerySchema { get; }

    public OpenApiDocument OpenApiDocument { get; }

    public OpenApiSchemaResolver OpenApiSchemaResolver { get; }

    internal Builder(IAppEntity app,
        OpenApiDocument document,
        OpenApiSchemaResolver schemaResolver,
        OpenApiSchemaGenerator schemaGenerator)
    {
        AppName = app.Name;

        OpenApiDocument = document;
        OpenApiSchemaResolver = schemaResolver;

        ChangeStatusSchema = CreateSchema<ChangeStatusDto>(schemaResolver, schemaGenerator);
        BulkRequestSchema = CreateSchema<BulkUpdateContentsDto>(schemaResolver, schemaGenerator);
        BulkResponseSchema = CreateSchema<BulkResultDto>(schemaResolver, schemaGenerator);
        QuerySchema = CreateSchema<QueryDto>(schemaResolver, schemaGenerator);
    }

    private static JsonSchema CreateSchema<T>(OpenApiSchemaResolver schemaResolver, OpenApiSchemaGenerator schemaGenerator)
    {
        var contextualType = typeof(T).ToContextualType();

        return schemaGenerator.GenerateWithReference<JsonSchema>(contextualType, schemaResolver);
    }

    public OperationsBuilder Shared()
    {
        var dataSchema = RegisterReference("DataDto", _ =>
        {
            return JsonSchema.CreateAnySchema();
        });

        var contentSchema = RegisterReference("ContentDto", _ =>
        {
            return ContentJsonSchema.Build(dataSchema, true);
        });

        var contentsSchema = RegisterReference("ContentResultDto", _ =>
        {
            return BuildResult(contentSchema);
        });

        var path = $"/api/content/{AppName}";

        var builder = new OperationsBuilder
        {
            ContentSchema = contentSchema,
            ContentsSchema = contentsSchema,
            DataSchema = dataSchema,
            Path = path,
            Parent = this,
            SchemaDisplayName = "__Shared",
            SchemaName = "__Shared",
            SchemaTypeName = "__Shared"
        };

        builder.AddTag("API endpoints for operations across all schemas.");

        return builder;
    }

    public OperationsBuilder Schema(Schema schema, PartitionResolver partitionResolver, ResolvedComponents components, bool flat)
    {
        var typeName = schema.TypeName();

        var dataSchema = RegisterReference($"{typeName}DataDto", _ =>
        {
            return schema.BuildJsonSchemaDynamic(partitionResolver, components, CreateReference, false, true);
        });

        var contentDataSchema = dataSchema;

        if (flat)
        {
            contentDataSchema = RegisterReference($"{typeName}FlatDataDto", _ =>
            {
                return schema.BuildJsonSchemaFlat(partitionResolver, components, CreateReference, false, true);
            });
        }

        var contentSchema = RegisterReference($"{typeName}ContentDto", _ =>
        {
            return ContentJsonSchema.Build(contentDataSchema, true);
        });

        var contentsSchema = RegisterReference($"{typeName}ContentResultDto", _ =>
        {
            return BuildResult(contentSchema);
        });

        var path = $"/api/content/{AppName}/{schema.Name}";

        var builder = new OperationsBuilder
        {
            ContentSchema = contentSchema,
            ContentsSchema = contentsSchema,
            DataSchema = dataSchema,
            Path = path,
            Parent = this,
            SchemaDisplayName = schema.DisplayName(),
            SchemaName = schema.Name,
            SchemaTypeName = typeName
        };

        builder.AddTag("API endpoints for [schema] content items.");

        return builder;
    }

    private JsonSchema RegisterReference(string name, Func<string, JsonSchema> creator)
    {
        name = char.ToUpperInvariant(name[0]) + name[1..];

        var reference = OpenApiDocument.Definitions.GetOrAdd(name, creator);

        return new JsonSchema
        {
            Reference = reference
        };
    }

    private (JsonSchema, JsonSchema?) CreateReference(string name)
    {
        name = char.ToUpperInvariant(name[0]) + name[1..];

        if (OpenApiDocument.Definitions.TryGetValue(name, out var definition))
        {
            var reference = new JsonSchema
            {
                Reference = definition
            };

            return (reference, null);
        }

        definition = JsonTypeBuilder.Object();

        OpenApiDocument.Definitions.Add(name, definition);

        return (new JsonSchema
        {
            Reference = definition
        }, definition);
    }

    private static JsonSchema BuildResult(JsonSchema contentSchema)
    {
        return new JsonSchema
        {
            AllowAdditionalProperties = false,
            Properties =
            {
                ["total"] = JsonTypeBuilder.NumberProperty(
                    FieldDescriptions.ContentsTotal, true),
                ["items"] = JsonTypeBuilder.ArrayProperty(contentSchema,
                    FieldDescriptions.ContentsItems, true)
            },
            Type = JsonObjectType.Object
        };
    }
}
