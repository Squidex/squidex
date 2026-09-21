// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps;

[EventType(nameof(AppClientSecretRegenerated))]
public sealed class AppClientSecretRegenerated : AppEvent
{
    public string Id { get; set; }

    public string Secret { get; set; }
}
