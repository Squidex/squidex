// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public interface IRuleRunnerService
    {
        Task RunAsync(Guid appId, Guid ruleId);

        Task CancelAsync(Guid appId);

        Task<Guid?> GetRunningRuleIdAsync(Guid appId);
    }
}
