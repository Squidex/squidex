// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents
{
    [EventType(nameof(AppClientChanged))]
    [Obsolete("New Event introduced")]
    public sealed class AppClientChanged : AppEvent, IMigrated<IEvent>
    {
        public string Id { get; set; }

        public bool IsReader { get; set; }

        public IEvent Migrate()
        {
            var permission =
                IsReader ?
                AppClientPermission.Reader :
                AppClientPermission.Editor;

            return SimpleMapper.Map(this, new AppClientUpdated { Permission = permission });
        }
    }
}
