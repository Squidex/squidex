// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using AppContributorAssignedV2 = Squidex.Domain.Apps.Events.Apps.AppContributorAssigned;

namespace Migrations.OldEvents
{
    [EventType(nameof(AppContributorAssigned))]
    [Obsolete("New Event introduced")]
    public sealed class AppContributorAssigned : AppEvent, IMigrated<IEvent>
    {
        public string ContributorId { get; set; }

        public AppContributorPermission Permission { get; set; }

        public IEvent Migrate()
        {
            var result = SimpleMapper.Map(this, new AppContributorAssignedV2());

            switch (Permission)
            {
                case AppContributorPermission.Owner:
                    result.Role = Role.Owner;
                    break;
                case AppContributorPermission.Developer:
                    result.Role = Role.Developer;
                    break;
                case AppContributorPermission.Editor:
                    result.Role = Role.Editor;
                    break;
            }

            return result;
        }
    }
}
