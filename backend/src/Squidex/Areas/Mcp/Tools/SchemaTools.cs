// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Squidex.Areas.Api.Controllers.Schemas.Models;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure.Json;
using Squidex.Shared;

namespace Squidex.Areas.Mcp.Tools;

[McpServerToolType]
public sealed class SchemaTools(IContextProvider contextProvider, IAppProvider appProvider, IJsonSerializer serializer)
    : McpToolBase(contextProvider, serializer)
{
    [McpServerTool(Name = "list_schemas", Title = "List schemas", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Lists all schemas of the app with name, type and label. Use get_schema to get the fields of a schema.")]
    public Task<string> ListSchemas(CancellationToken ct)
    {
        return ExecuteAsync(async context =>
        {
            EnsurePermission(context, PermissionIds.AppSchemasRead);

            var schemas = await appProvider.GetSchemasAsync(context.App.Id, ct);

            return schemas.OrderBy(x => x.Name).Select(x => new
            {
                x.Name,
                x.Type,
                x.Category,
                x.IsPublished,
                x.Properties.Label,
                x.Properties.Hints,
            }).ToList();
        });
    }

    [McpServerTool(Name = "get_schema", Title = "Get schema", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Gets a schema with all fields. The field names are the keys in the data of content items.")]
    public Task<string> GetSchema(
        [Description("The name or ID of the schema.")] string schema,
        CancellationToken ct = default)
    {
        return ExecuteAsync(async context =>
        {
            EnsurePermission(context, PermissionIds.AppSchemasRead);

            var result = await appProvider.GetSchemaAsync(context.App.Id, schema, true, ct)
                ?? throw new McpException($"Schema '{schema}' not found.");

            return new
            {
                result.Name,
                result.Type,
                result.Category,
                result.IsPublished,
                result.Properties.Label,
                result.Properties.Hints,
                Fields = result.Fields.Select(FieldDto.FromDomain).ToList(),
            };
        });
    }
}
