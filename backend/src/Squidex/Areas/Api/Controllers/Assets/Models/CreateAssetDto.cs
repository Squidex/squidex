// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

[OpenApiRequest]
public sealed class CreateAssetDto
{
    /// <summary>
    /// The file to upload.
    /// </summary>
    [FromForm(Name = "file2")]
    public IFormFile File2 { get; set; }

    /// <summary>
    /// The file to upload.
    /// </summary>
    [FromForm(Name = "file")]
    public IAssetFile File { get; set; }

    /// <summary>
    /// The optional parent folder id.
    /// </summary>
    // [FromQuery(Name = "parentId")]
    public DomainId ParentId { get; set; }

    /// <summary>
    /// The optional custom asset id.
    /// </summary>
    [FromQuery(Name = "id")]
    public DomainId? Id { get; set; }

    /// <summary>
    /// True to duplicate the asset, event if the file has been uploaded.
    /// </summary>
    [FromQuery(Name = "duplicate")]
    public bool Duplicate { get; set; }

    public CreateAsset ToCommand()
    {
        var command = SimpleMapper.Map(this, new CreateAsset());

        if (Id != null && Id.Value != default && !string.IsNullOrWhiteSpace(Id.Value.ToString()))
        {
            command.AssetId = Id.Value;
        }

        return command;
    }
}
