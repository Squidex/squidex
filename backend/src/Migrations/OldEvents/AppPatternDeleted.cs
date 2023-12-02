// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents;

[EventType(nameof(AppPatternDeleted))]
[Obsolete("New Event introduced")]
public sealed class AppPatternDeleted : AppEvent, IMigratedStateEvent<App>
{
    public DomainId PatternId { get; set; }

    public IEvent Migrate(App state)
    {
        var newEvent = new AppSettingsUpdated
        {
            Settings = state.Settings
        };

        return SimpleMapper.Map(this, newEvent);
    }
}
