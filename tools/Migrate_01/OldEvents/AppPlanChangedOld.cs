// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.OldEvents
{
    [TypeName("AppPlanChanged")]
    public sealed class AppPlanChangedOld : AppEvent, IMigrated<IEvent>
    {
        public string PlanId { get; set; }

        public IEvent Migrate()
        {
            if (!string.IsNullOrWhiteSpace(PlanId))
            {
                return SimpleMapper.Map(this, new AppPlanChanged());
            }
            else
            {
                return SimpleMapper.Map(this, new AppPlanReset());
            }
        }
    }
}
