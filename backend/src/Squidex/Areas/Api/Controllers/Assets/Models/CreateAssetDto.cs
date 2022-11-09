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

namespace Squidex.Areas.Api.Controllers.Assets.Models;

public sealed class CreateAssetDto
{
    /// <summary>
    /// The file to upload.
    /// </summary>
    public IFormFile File { get; set; }

    /// <summary>
    /// The optional parent folder id.
    /// </summary>
    [FromQuery]
    public DomainId ParentId { get; set; }

    /// <summary>
    /// The optional custom asset id.
    /// </summary>
    [FromQuery]
    public DomainId? Id { get; set; }

    /// <summary>
    /// True to duplicate the asset, event if the file has been uploaded.
    /// </summary>
    [FromQuery]
    public bool Duplicate { get; set; }

    public CreateAsset ToCommand(AssetFile file)
    {
        var command = SimpleMapper.Map(this, new CreateAsset { File = file });

        if (Id != null && Id.Value != default && !string.IsNullOrWhiteSpace(Id.Value.ToString()))
        {
            command.AssetId = Id.Value;
        }

        return command;
    }
}
