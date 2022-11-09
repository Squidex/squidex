// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets;

[EventType(nameof(AssetCreated), 2)]
public sealed class AssetCreated : AssetEvent
{
    public DomainId ParentId { get; set; }

    public string FileName { get; set; }

    public string FileHash { get; set; }

    public string MimeType { get; set; }

    public string Slug { get; set; }

    public long FileVersion { get; set; }

    public long FileSize { get; set; }

    public AssetType Type { get; set; }

    public AssetMetadata Metadata { get; set; }

    public HashSet<string>? Tags { get; set; }
}
