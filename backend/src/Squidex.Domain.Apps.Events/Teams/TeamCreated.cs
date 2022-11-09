// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Teams;

[EventType(nameof(TeamCreated))]
public sealed class TeamCreated : TeamEvent
{
    public string Name { get; set; }
}
