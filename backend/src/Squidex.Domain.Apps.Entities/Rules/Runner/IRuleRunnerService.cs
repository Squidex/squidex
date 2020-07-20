// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public interface IRuleRunnerService
    {
        Task RunAsync(DomainId appId, DomainId ruleId);

        Task CancelAsync(DomainId appId);

        Task<DomainId?> GetRunningRuleIdAsync(DomainId appId);
    }
}
