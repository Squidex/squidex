// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public interface IRuleRunnerGrain : IGrainWithStringKey
    {
        Task RunAsync(DomainId ruleId, bool fromSnapshots);

        Task CancelAsync();

        Task<DomainId?> GetRunningRuleIdAsync();
    }
}
