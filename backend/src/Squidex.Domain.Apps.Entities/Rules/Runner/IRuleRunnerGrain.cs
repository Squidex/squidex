// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public interface IRuleRunnerGrain : IGrainWithGuidKey
    {
        Task RunAsync(Guid ruleId);

        Task CancelAsync();

        Task<Guid?> GetRunningRuleIdAsync();
    }
}
