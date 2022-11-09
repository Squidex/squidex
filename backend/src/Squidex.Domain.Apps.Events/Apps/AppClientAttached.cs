// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps;

[EventType(nameof(AppClientAttached))]
public sealed class AppClientAttached : AppEvent
{
    public string Id { get; set; }

    public string Secret { get; set; }

    public string? Role { get; set; }
}
