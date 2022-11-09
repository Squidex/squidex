// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using NJsonSchema;
using NSwag;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Contents.Generator;

internal sealed class OperationBuilder
{
    private readonly OpenApiOperation operation = new OpenApiOperation();
    private readonly OperationsBuilder operations;

    public OperationBuilder(OperationsBuilder operations, OpenApiOperation operation)
    {
        this.operations = operations;
        this.operation = operation;
    }

    public OperationBuilder Operation(string name)
    {
        operation.OperationId = $"{name}{operations.SchemaTypeName}Content";

        return this;
    }

    public OperationBuilder OperationSummary(string summary)
    {
        if (!string.IsNullOrWhiteSpace(summary))
        {
            operation.Summary = operations.FormatText(summary);
        }

        return this;
    }

    public OperationBuilder Describe(string description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            operation.Description = description;
        }

        return this;
    }

    public OperationBuilder HasId()
    {
        HasPath("id", JsonObjectType.String, FieldDescriptions.EntityId);

        Responds(404, "Content item not found.");

        return this;
    }

    private OperationBuilder AddParameter(string name, JsonSchema schema, OpenApiParameterKind kind, string? description)
    {
        var parameter = new OpenApiParameter
        {
            Kind = kind,
            Schema = schema,
            Name = name,
        };

        if (!string.IsNullOrWhiteSpace(description))
        {
            parameter.Description = operations.FormatText(description);
        }

        if (kind != OpenApiParameterKind.Query)
        {
            parameter.IsRequired = true;
            parameter.IsNullableRaw = false;
        }

        operation.Parameters.Add(parameter);

        return this;
    }

    public OperationBuilder HasQueryOptions(bool supportSearch)
    {
        operation.AddQuery(true);

        return this;
    }

    public OperationBuilder Deprecated()
    {
        operation.IsDeprecated = true;

        return this;
    }

    public OperationBuilder HasQuery(string name, JsonObjectType type, string description)
    {
        var jsonSchema = new JsonSchema { Type = type };

        return AddParameter(name, jsonSchema, OpenApiParameterKind.Query, description);
    }

    public OperationBuilder HasPath(string name, JsonObjectType type, string description, string? format = null)
    {
        var jsonSchema = new JsonSchema { Type = type, Format = format };

        return AddParameter(name, jsonSchema, OpenApiParameterKind.Path, description);
    }

    public OperationBuilder HasBody(string name, JsonSchema schema, string? description = null)
    {
        var jsonSchema = schema;

        return AddParameter(name, jsonSchema, OpenApiParameterKind.Body, description);
    }

    public OperationBuilder Responds(int statusCode, string description, JsonSchema? schema = null)
    {
        var response = new OpenApiResponse
        {
            Description = description
        };

        if (schema != null && statusCode == 204)
        {
            ThrowHelper.ArgumentException("Invalid status code.", nameof(statusCode));
        }

        response.Schema = schema;

        operation.Responses.Add(statusCode.ToString(CultureInfo.InvariantCulture), response);

        return this;
    }

    public OperationBuilder RequirePermission(string permissionId)
    {
        var fullId = PermissionIds.ForApp(permissionId, operations.Parent.AppName, operations.SchemaName).Id;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                [Constants.SecurityDefinition] = new[] { fullId }
            }
        };

        return this;
    }
}
