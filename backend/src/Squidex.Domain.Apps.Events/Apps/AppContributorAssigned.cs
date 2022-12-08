// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps;

[EventType(nameof(AppContributorAssigned), 2)]
public sealed class AppContributorAssigned : AppEvent
{
    public string ContributorId { get; set; }

    public string Role { get; set; }

    public bool IsCreated { get; set; }

    public bool IsAdded { get; set; }
}
