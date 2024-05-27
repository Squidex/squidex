// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

[OpenApiRequest]
public sealed class UpsertAssetDto : UploadModel
{
    /// <summary>
    /// The optional parent folder id.
    /// </summary>
    [FromQuery(Name = "parentId")]
    public DomainId ParentId { get; set; }

    /// <summary>
    /// True to duplicate the asset, event if the file has been uploaded.
    /// </summary>
    [FromQuery(Name = "duplicate")]
    public bool Duplicate { get; set; }

    public static UpsertAsset ToCommand(AssetTusFile file)
    {
        var command = new UpsertAsset { File = file };

        bool TryGetString(string key, out string result)
        {
            result = null!;

            var value = file.Metadata.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value;

            if (!string.IsNullOrWhiteSpace(value))
            {
                result = value;
                return true;
            }

            return false;
        }

        if (TryGetString("id", out var id))
        {
            command.AssetId = DomainId.Create(id);
        }

        if (TryGetString("parentId", out var parentId))
        {
            command.ParentId = DomainId.Create(parentId);
        }

        if (TryGetString("duplicate", out var duplicate) && bool.TryParse(duplicate, out var parsed))
        {
            command.Duplicate = parsed;
        }

        return command;
    }

    public async Task<UpsertAsset> ToCommandAsync(DomainId id, HttpContext httpContext, App app)
    {
        var file = await ToFileAsync(httpContext, app);

        return SimpleMapper.Map(this, new UpsertAsset { AssetId = id, File = file });
    }
}
