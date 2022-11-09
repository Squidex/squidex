// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets;

[EventType(nameof(AssetFolderCreated))]
public sealed class AssetFolderCreated : AssetFolderEvent
{
    public DomainId ParentId { get; set; }

    public string FolderName { get; set; }
}
