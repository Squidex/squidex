// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets;

[EventType(nameof(AssetFolderRenamed))]
public sealed class AssetFolderRenamed : AssetFolderEvent
{
    public string FolderName { get; set; }
}
