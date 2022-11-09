// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using AppClientUpdatedV2 = Squidex.Domain.Apps.Events.Apps.AppClientUpdated;

namespace Migrations.OldEvents;

[EventType(nameof(AppClientUpdated))]
[Obsolete("New Event introduced")]
public sealed class AppClientUpdated : AppEvent, IMigrated<IEvent>
{
    public string Id { get; set; }

    public AppClientPermission Permission { get; set; }

    public IEvent Migrate()
    {
        var result = SimpleMapper.Map(this, new AppClientUpdatedV2());

        switch (Permission)
        {
            case AppClientPermission.Developer:
                result.Role = Role.Developer;
                break;
            case AppClientPermission.Editor:
                result.Role = Role.Editor;
                break;
            case AppClientPermission.Reader:
                result.Role = Role.Reader;
                break;
        }

        return result;
    }
}