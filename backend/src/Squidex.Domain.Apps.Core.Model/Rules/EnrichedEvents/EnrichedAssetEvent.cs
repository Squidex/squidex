// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

public sealed class EnrichedAssetEvent : EnrichedUserEventBase, IEnrichedEntityEvent
{
    [FieldDescription(nameof(FieldDescriptions.EventType))]
    public EnrichedAssetEventType Type { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityId))]
    public DomainId Id { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityCreated))]
    public Instant Created { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityLastModified))]
    public Instant LastModified { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityCreatedBy))]
    public RefToken CreatedBy { get; set; }

    [FieldDescription(nameof(FieldDescriptions.EntityLastModifiedBy))]
    public RefToken LastModifiedBy { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetParentId))]
    public DomainId ParentId { get; }

    [FieldDescription(nameof(FieldDescriptions.AssetMimeType))]
    public string MimeType { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetFileName))]
    public string FileName { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetFileHash))]
    public string FileHash { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetSlug))]
    public string Slug { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetFileVersion))]
    public long FileVersion { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetFileSize))]
    public long FileSize { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetIsProtected))]
    public bool IsProtected { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetPixelWidth))]
    public int? PixelWidth { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetPixelHeight))]
    public int? PixelHeight { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetType))]
    public AssetType AssetType { get; set; }

    [FieldDescription(nameof(FieldDescriptions.AssetMetadata))]
    public AssetMetadata Metadata { get; }

    [FieldDescription(nameof(FieldDescriptions.AssetIsImage))]
    public bool IsImage
    {
        get => AssetType == AssetType.Image;
    }

    public override long Partition
    {
        get => Id.GetHashCode();
    }
}
