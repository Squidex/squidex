// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.HandleRules;

public interface IRuleService
{
    bool CanCreateSnapshotEvents(Rule rule);

    string GetName(AppEvent @event);

    Task CreateSnapshotJobsAsync(JobCallback callback, RuleContext context,
        CancellationToken ct = default);

    Task CreateJobsAsync(JobCallback callback, Envelope<IEvent> @event, RulesContext context,
        CancellationToken ct = default);

    Task<(Result Result, TimeSpan Elapsed)> InvokeAsync(string actionName, string job,
        CancellationToken ct = default);
}

public delegate ValueTask JobCallback(DomainId ruleId, Rule rule, JobResult result, CancellationToken ct);
