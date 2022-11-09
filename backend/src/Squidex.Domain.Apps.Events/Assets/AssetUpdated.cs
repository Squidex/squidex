// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets;

[EventType(nameof(AssetUpdated), 2)]
public sealed class AssetUpdated : AssetEvent
{
    public string MimeType { get; set; }

    public string FileHash { get; set; }

    public long FileSize { get; set; }

    public long FileVersion { get; set; }

    public AssetType Type { get; set; }

    public AssetMetadata Metadata { get; set; }
}
