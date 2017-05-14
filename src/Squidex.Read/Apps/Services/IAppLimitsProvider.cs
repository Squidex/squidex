// ==========================================================================
//  IAppLimitsProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Read.Apps.Services
{
    public interface IAppLimitsProvider
    {
        IEnumerable<IAppLimitsPlan> GetAvailablePlans();

        IAppLimitsPlan GetPlanForApp(IAppEntity entity);

        IAppLimitsPlan GetPlan(int planId);
    }
}
