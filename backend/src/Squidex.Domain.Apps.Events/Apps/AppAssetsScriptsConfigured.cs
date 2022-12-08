// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps;

[EventType(nameof(AppAssetsScriptsConfigured))]
public sealed class AppAssetsScriptsConfigured : AppEvent
{
    public AssetScripts? Scripts { get; set; }
}
