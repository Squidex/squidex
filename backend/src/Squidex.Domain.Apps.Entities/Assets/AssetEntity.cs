// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetEntity : IEnrichedAssetEntity
{
    public NamedId<DomainId> AppId { get; set; }

    public DomainId Id { get; set; }

    public DomainId ParentId { get; set; }

    public Instant Created { get; set; }

    public Instant LastModified { get; set; }

    public RefToken CreatedBy { get; set; }

    public RefToken LastModifiedBy { get; set; }

    public HashSet<string> Tags { get; set; }

    public HashSet<string> TagNames { get; set; }

    public long Version { get; set; }

    public string FileName { get; set; }

    public string FileHash { get; set; }

    public string MimeType { get; set; }

    public string Slug { get; set; }

    public string MetadataText { get; set; }

    public string? EditToken { get; set; }

    public long FileSize { get; set; }

    public long FileVersion { get; set; }

    public bool IsProtected { get; set; }

    public bool IsDeleted { get; set; }

    public AssetMetadata Metadata { get; set; }

    public AssetType Type { get; set; }

    public DomainId AssetId
    {
        get => Id;
    }

    public DomainId UniqueId
    {
        get => DomainId.Combine(AppId, Id);
    }
}
