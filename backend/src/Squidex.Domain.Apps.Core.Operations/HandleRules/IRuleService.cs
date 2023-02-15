// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules;

public interface IRuleService
{
    bool CanCreateSnapshotEvents(Rule rule);

    string GetName(AppEvent @event);

    IAsyncEnumerable<JobResult> CreateSnapshotJobsAsync(RuleContext context,
        CancellationToken ct = default);

    IAsyncEnumerable<JobResult> CreateJobsAsync(Envelope<IEvent> @event, RulesContext context,
        CancellationToken ct = default);

    Task<(Result Result, TimeSpan Elapsed)> InvokeAsync(string actionName, string job,
        CancellationToken ct = default);
}
