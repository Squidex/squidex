// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Apps.Services
{
    public interface IAppPlansProvider
    {
        IEnumerable<IAppLimitsPlan> GetAvailablePlans();

        bool IsConfiguredPlan(string planId);

        IAppLimitsPlan GetPlanUpgradeForApp(IAppEntity app);

        IAppLimitsPlan GetPlanUpgrade(string planId);

        IAppLimitsPlan GetPlanForApp(IAppEntity app);

        IAppLimitsPlan GetPlan(string planId);
    }
}
