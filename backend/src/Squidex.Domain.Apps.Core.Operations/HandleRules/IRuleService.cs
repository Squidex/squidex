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

    IAsyncEnumerable<JobResult> CreateSnapshotJobsAsync(RuleContext context,
        CancellationToken ct = default);

    IAsyncEnumerable<JobResult> CreateJobsAsync(Envelope<AppEvent> @event, RulesContext context,
        CancellationToken ct = default);
}
