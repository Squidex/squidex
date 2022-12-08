// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Teams;

[EventType(nameof(TeamUpdated))]
public sealed class TeamUpdated : TeamEvent
{
    public string Name { get; set; }
}
