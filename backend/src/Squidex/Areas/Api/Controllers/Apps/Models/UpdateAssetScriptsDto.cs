// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class UpdateAssetScriptsDto
{
    /// <summary>
    /// The script that is executed when creating an asset.
    /// </summary>
    public string? Create { get; init; }

    /// <summary>
    /// The script that is executed when updating a content.
    /// </summary>
    public string? Update { get; init; }

    /// <summary>
    /// The script that is executed when annotating a content.
    /// </summary>
    public string? Annotate { get; init; }

    /// <summary>
    /// The script that is executed when moving a content.
    /// </summary>
    public string? Move { get; init; }

    /// <summary>
    /// The script that is executed when deleting a content.
    /// </summary>
    public string? Delete { get; init; }

    public ConfigureAssetScripts ToCommand()
    {
        var scripts = SimpleMapper.Map(this, new AssetScripts());

        return new ConfigureAssetScripts { Scripts = scripts };
    }
}
