// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure.Json;

namespace Squidex.Areas.Mcp.Tools;

[McpServerToolType]
public sealed class ContentTools(IContextProvider contextProvider, IContentQueryService contentQuery, IJsonSerializer serializer)
    : McpToolBase(contextProvider, serializer)
{
    [McpServerTool(Name = "query_contents", Title = "Query contents", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("""
        Queries content items of a schema. Use either a JSON query or an OData query. If both are provided, the JSON query is used.
        JSON query example: {"filter": {"path": "data.title.iv", "op": "contains", "value": "hello"}, "sort": [{"path": "lastModified", "order": "descending"}], "skip": 0, "take": 20}.
        OData query example: $filter=contains(data/title/iv, 'hello')&$orderby=lastModified desc&$skip=0&$top=20.
        Field values are stored per language or with the "iv" key for non-localized fields. Use get_schema to get the field names.
        """)]
    public Task<string> QueryContents(
        [Description("The name or ID of the schema.")] string schema,
        [Description("The optional JSON query.")] string? query = null,
        [Description("The optional OData query, e.g. $filter=data/title/iv eq 'hello'&$top=10.")] string? odata = null,
        [Description("True to also return unpublished content items.")] bool unpublished = false,
        CancellationToken ct = default)
    {
        return ExecuteAsync(async context =>
        {
            var q = Q.Empty.WithJsonQuery(query).WithODataQuery(odata);

            var contents = await contentQuery.QueryAsync(WithOptions(context, unpublished), schema, q, ct);

            return new
            {
                contents.Total,
                Items = contents.Select(ToResult).ToList(),
            };
        });
    }

    [McpServerTool(Name = "get_content", Title = "Get content", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Gets a single content item by ID.")]
    public Task<string> GetContent(
        [Description("The name or ID of the schema.")] string schema,
        [Description("The ID of the content item.")] string id,
        [Description("True to also return unpublished content items.")] bool unpublished = false,
        CancellationToken ct = default)
    {
        return ExecuteAsync(async context =>
        {
            var content = await contentQuery.FindAsync(WithOptions(context, unpublished), schema, ParseId(id), ct: ct)
                ?? throw new McpException($"Content '{id}' not found.");

            return ToResult(content);
        });
    }

    private static Context WithOptions(Context context, bool unpublished)
    {
        return context.Clone(b => b.WithUnpublished(unpublished));
    }

    private static object ToResult(EnrichedContent content)
    {
        return new
        {
            content.Id,
            Schema = content.SchemaId.Name,
            content.Status,
            content.NewStatus,
            content.Version,
            content.Created,
            content.LastModified,
            content.Data,
        };
    }
}
