// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public interface IRuleRunnerService
{
    Task<List<SimulatedRuleEvent>> SimulateAsync(NamedId<DomainId> appId, DomainId ruleId, Rule rule,
        CancellationToken ct = default);

    Task<List<SimulatedRuleEvent>> SimulateAsync(IRuleEntity rule,
        CancellationToken ct = default);

    Task RunAsync(DomainId appId, DomainId ruleId, bool fromSnapshots = false,
        CancellationToken ct = default);

    Task CancelAsync(DomainId appId,
        CancellationToken ct = default);

    Task<DomainId?> GetRunningRuleIdAsync(DomainId appId,
        CancellationToken ct = default);

    bool CanRunRule(IRuleEntity rule);

    bool CanRunFromSnapshots(IRuleEntity rule);
}
