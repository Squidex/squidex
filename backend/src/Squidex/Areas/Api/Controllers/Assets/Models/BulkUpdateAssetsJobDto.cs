// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

public class BulkUpdateAssetsJobDto
{
    /// <summary>
    /// An optional ID of the asset to update.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The update type.
    /// </summary>
    public BulkUpdateAssetType Type { get; set; }

    /// <summary>
    /// The parent folder id.
    /// </summary>
    public DomainId ParentId { get; set; }

    /// <summary>
    /// The new name of the asset.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// The new slug of the asset.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// True, when the asset is not public.
    /// </summary>
    public bool? IsProtected { get; set; }

    /// <summary>
    /// The new asset tags.
    /// </summary>
    public HashSet<string>? Tags { get; set; }

    /// <summary>
    /// The asset metadata.
    /// </summary>
    public AssetMetadata? Metadata { get; set; }

    /// <summary>
    /// True to delete the asset permanently.
    /// </summary>
    public bool Permanent { get; set; }

    /// <summary>
    /// The expected version.
    /// </summary>
    public long ExpectedVersion { get; set; } = EtagVersion.Any;

    public BulkUpdateJob ToJob()
    {
        return SimpleMapper.Map(this, new BulkUpdateJob());
    }
}
