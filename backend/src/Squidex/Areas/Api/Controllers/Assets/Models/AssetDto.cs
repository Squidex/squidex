// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

public sealed class AssetDto : Resource
{
    /// <summary>
    /// The ID of the asset.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The ID of the parent folder. Empty for files without parent.
    /// </summary>
    public DomainId ParentId { get; set; }

    /// <summary>
    /// The file name.
    /// </summary>
    [LocalizedRequired]
    public string FileName { get; set; }

    /// <summary>
    /// The file hash.
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// True, when the asset is not public.
    /// </summary>
    public bool IsProtected { get; set; }

    /// <summary>
    /// The slug.
    /// </summary>
    [LocalizedRequired]
    public string Slug { get; set; }

    /// <summary>
    /// The mime type.
    /// </summary>
    [LocalizedRequired]
    public string MimeType { get; set; }

    /// <summary>
    /// The file type.
    /// </summary>
    [LocalizedRequired]
    public string FileType { get; set; }

    /// <summary>
    /// The formatted text representation of the metadata.
    /// </summary>
    [LocalizedRequired]
    public string MetadataText { get; set; }

    /// <summary>
    /// The UI token.
    /// </summary>
    public string? EditToken { get; set; }

    /// <summary>
    /// The asset metadata.
    /// </summary>
    [LocalizedRequired]
    public AssetMetadata Metadata { get; set; }

    /// <summary>
    /// The asset tags.
    /// </summary>
    [LocalizedRequired]
    public HashSet<string>? Tags { get; set; }

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// The version of the file.
    /// </summary>
    public long FileVersion { get; set; }

    /// <summary>
    /// The type of the asset.
    /// </summary>
    public AssetType Type { get; set; }

    /// <summary>
    /// The user that has created the schema.
    /// </summary>
    [LocalizedRequired]
    public RefToken CreatedBy { get; set; }

    /// <summary>
    /// The user that has updated the asset.
    /// </summary>
    [LocalizedRequired]
    public RefToken LastModifiedBy { get; set; }

    /// <summary>
    /// The date and time when the asset has been created.
    /// </summary>
    public Instant Created { get; set; }

    /// <summary>
    /// The date and time when the asset has been modified last.
    /// </summary>
    public Instant LastModified { get; set; }

    /// <summary>
    /// The version of the asset.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// The metadata.
    /// </summary>
    [JsonPropertyName("_meta")]
    public AssetMeta? Meta { get; set; }

    /// <summary>
    /// Determines of the created file is an image.
    /// </summary>
    [Obsolete("Use 'type' field now.")]
    public bool IsImage
    {
        get => Type == AssetType.Image;
    }

    /// <summary>
    /// The width of the image in pixels if the asset is an image.
    /// </summary>
    [Obsolete("Use 'metadata' field now.")]
    public int? PixelWidth
    {
        get => Metadata.GetPixelWidth();
    }

    /// <summary>
    /// The height of the image in pixels if the asset is an image.
    /// </summary>
    [Obsolete("Use 'metadata' field now.")]
    public int? PixelHeight
    {
        get => Metadata.GetPixelHeight();
    }

    public static AssetDto FromDomain(IEnrichedAssetEntity asset, Resources resources, bool isDuplicate = false)
    {
        var result = SimpleMapper.Map(asset, new AssetDto());

        result.Tags = asset.TagNames;

        if (isDuplicate)
        {
            result.Meta = new AssetMeta
            {
                IsDuplicate = "true"
            };
        }

        result.FileType = asset.FileName.FileType();

        return result.CreateLinks(resources);
    }

    private AssetDto CreateLinks(Resources resources)
    {
        var app = resources.App;

        var values = new { app, id = Id };

        AddSelfLink(resources.Url<AssetsController>(x => nameof(x.GetAsset), values));

        if (resources.CanUpdateAsset)
        {
            AddPutLink("update",
                resources.Url<AssetsController>(x => nameof(x.PutAsset), values));
        }

        if (resources.CanUpdateAsset)
        {
            AddPutLink("move",
                resources.Url<AssetsController>(x => nameof(x.PutAssetParent), values));
        }

        if (resources.CanUploadAsset)
        {
            AddPutLink("upload",
                resources.Url<AssetsController>(x => nameof(x.PutAssetContent), values));
        }

        if (resources.CanDeleteAsset)
        {
            AddDeleteLink("delete",
                resources.Url<AssetsController>(x => nameof(x.DeleteAsset), values));
        }

        if (!string.IsNullOrWhiteSpace(Slug))
        {
            var idValues = new { app, idOrSlug = Id, more = Slug };

            AddGetLink("content",
                resources.Url<AssetContentController>(x => nameof(x.GetAssetContentBySlug), idValues));

            var slugValues = new { app, idOrSlug = Slug };

            AddGetLink("content/slug",
                resources.Url<AssetContentController>(x => nameof(x.GetAssetContentBySlug), slugValues));
        }
        else
        {
            var idValues = new { app, idOrSlug = Id };

            AddGetLink("content",
                resources.Url<AssetContentController>(x => nameof(x.GetAssetContentBySlug), idValues));
        }

        return this;
    }
}
