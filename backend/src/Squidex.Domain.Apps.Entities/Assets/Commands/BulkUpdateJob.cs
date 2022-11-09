// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands;

public sealed class BulkUpdateJob
{
    public BulkUpdateAssetType Type { get; set; }

    public DomainId Id { get; set; }

    public DomainId ParentId { get; set; }

    public string? FileName { get; set; }

    public string? Slug { get; set; }

    public bool? IsProtected { get; set; }

    public bool Permanent { get; set; }

    public HashSet<string> Tags { get; set; }

    public AssetMetadata? Metadata { get; set; }

    public long ExpectedVersion { get; set; } = EtagVersion.Any;
}
