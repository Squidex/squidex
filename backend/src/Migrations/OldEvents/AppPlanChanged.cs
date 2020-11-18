// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using AppPlanChangedV2 = Squidex.Domain.Apps.Events.Apps.AppPlanChanged;

namespace Migrations.OldEvents
{
    [TypeName("AppPlanChanged")]
    [Obsolete("New Event introduced")]
    public sealed class AppPlanChanged : AppEvent, IMigrated<IEvent>
    {
        public string PlanId { get; set; }

        public IEvent Migrate()
        {
            if (!string.IsNullOrWhiteSpace(PlanId))
            {
                return SimpleMapper.Map(this, new AppPlanChangedV2());
            }
            else
            {
                return SimpleMapper.Map(this, new AppPlanReset());
            }
        }
    }
}
