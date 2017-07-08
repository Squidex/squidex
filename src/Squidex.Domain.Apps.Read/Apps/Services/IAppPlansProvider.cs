// ==========================================================================
//  IAppPlansProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Read.Apps.Services
{
    public interface IAppPlansProvider
    {
        IEnumerable<IAppLimitsPlan> GetAvailablePlans();

        bool IsConfiguredPlan(string planId);

        IAppLimitsPlan GetPlanForApp(IAppEntity entity);

        IAppLimitsPlan GetPlan(string planId);
    }
}
