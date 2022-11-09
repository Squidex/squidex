// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents;

[EventType(nameof(AppPatternDeleted))]
[Obsolete("New Event introduced")]
public sealed class AppPatternDeleted : AppEvent, IMigratedStateEvent<AppDomainObject.State>
{
    public DomainId PatternId { get; set; }

    public IEvent Migrate(AppDomainObject.State state)
    {
        var newEvent = new AppSettingsUpdated
        {
            Settings = state.Settings
        };

        return SimpleMapper.Map(this, newEvent);
    }
}
