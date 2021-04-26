// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public interface IAppPlansProvider
    {
        IEnumerable<IAppLimitsPlan> GetAvailablePlans();

        bool IsConfiguredPlan(string? planId);

        IAppLimitsPlan? GetPlanUpgradeForApp(IAppEntity app);

        IAppLimitsPlan? GetPlanUpgrade(string? planId);

        IAppLimitsPlan? GetPlan(string? planId);

        IAppLimitsPlan GetFreePlan();

        (IAppLimitsPlan Plan, string PlanId) GetPlanForApp(IAppEntity app);
    }
}
