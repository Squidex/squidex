// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Shared;

namespace Squidex.Areas.Mcp.Tools;

[McpServerToolType]
public sealed class AssetTools(IContextProvider contextProvider, IAssetQueryService assetQuery, IUrlGenerator urlGenerator, IJsonSerializer serializer)
    : McpToolBase(contextProvider, serializer)
{
    [McpServerTool(Name = "query_assets", Title = "Query assets", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("""
        Queries assets. Use either a JSON query or an OData query. If both are provided, the JSON query is used.
        JSON query example: {"filter": {"path": "fileName", "op": "contains", "value": "logo"}, "sort": [{"path": "lastModified", "order": "descending"}], "take": 20}.
        OData query example: $filter=contains(fileName, 'logo')&$orderby=lastModified desc&$top=20.
        """)]
    public Task<string> QueryAssets(
        [Description("The optional JSON query.")] string? query = null,
        [Description("The optional OData query, e.g. $filter=mimeType eq 'image/png'&$top=10.")] string? odata = null,
        [Description("The optional ID of the folder. Omit to search in all folders.")] string? parentId = null,
        CancellationToken ct = default)
    {
        return ExecuteAsync(async context =>
        {
            EnsurePermission(context, PermissionIds.AppAssetsRead);

            var parent = string.IsNullOrWhiteSpace(parentId) ? (DomainId?)null : ParseId(parentId);

            var q = Q.Empty.WithJsonQuery(query).WithODataQuery(odata);

            var assets = await assetQuery.QueryAsync(context, parent, q, ct);

            return new
            {
                assets.Total,
                Items = assets.Select(ToResult).ToList(),
            };
        });
    }

    [McpServerTool(Name = "get_asset", Title = "Get asset", ReadOnly = true, Idempotent = true, OpenWorld = false)]
    [Description("Gets the metadata of a single asset by ID, including the URL to download the file.")]
    public Task<string> GetAsset(
        [Description("The ID of the asset.")] string id,
        CancellationToken ct = default)
    {
        return ExecuteAsync(async context =>
        {
            EnsurePermission(context, PermissionIds.AppAssetsRead);

            var asset = await assetQuery.FindAsync(context, ParseId(id), ct: ct)
                ?? throw new McpException($"Asset '{id}' not found.");

            return ToResult(asset);
        });
    }

    private object ToResult(EnrichedAsset asset)
    {
        return new
        {
            asset.Id,
            asset.ParentId,
            asset.FileName,
            asset.FileSize,
            asset.MimeType,
            asset.Slug,
            asset.Type,
            asset.IsProtected,
            asset.Metadata,
            Tags = asset.TagNames,
            asset.Version,
            asset.Created,
            asset.LastModified,
            Url = urlGenerator.AssetContent(asset.AppId, asset.Id.ToString()),
        };
    }
}
