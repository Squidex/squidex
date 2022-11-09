// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents;

[EventType(nameof(AppPatternAdded))]
[Obsolete("New Event introduced")]
public sealed class AppPatternAdded : AppEvent, IMigratedStateEvent<AppDomainObject.State>
{
    public DomainId PatternId { get; set; }

    public string Name { get; set; }

    public string Pattern { get; set; }

    public string? Message { get; set; }

    public IEvent Migrate(AppDomainObject.State state)
    {
        var newSettings = state.Settings with
        {
            Patterns = new List<Pattern>(state.Settings.Patterns.Where(x => x.Name != Name || x.Regex != Pattern))
            {
                new Pattern(Name, Pattern)
                {
                    Message = Message
                }
            }.ToReadonlyList()
        };

        var newEvent = new AppSettingsUpdated
        {
            Settings = newSettings
        };

        return SimpleMapper.Map(this, newEvent);
    }
}
