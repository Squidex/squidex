// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using AppClientUpdatedV2 = Squidex.Domain.Apps.Events.Apps.AppClientUpdated;

namespace Migrations.OldEvents
{
    [EventType(nameof(AppClientRenamed))]
    public sealed class AppClientRenamed : AppEvent, IMigrated<IEvent>
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IEvent Migrate()
        {
            var result = SimpleMapper.Map(this, new AppClientUpdatedV2());

            return result;
        }
    }
}
