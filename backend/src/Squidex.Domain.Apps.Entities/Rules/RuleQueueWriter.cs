// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;

namespace Squidex.Domain.Apps.Entities.Rules;

internal sealed class RuleQueueWriter : IAsyncDisposable
{
    private readonly List<RuleEventWrite> writes = new List<RuleEventWrite>();
    private readonly IRuleEventRepository repository;

    public RuleQueueWriter(IRuleEventRepository repository)
    {
        this.repository = repository;
    }

    public async Task<bool> WriteAsync(JobResult result, ILogger? log = null)
    {
        // We do not want to handle disabled rules in the normal flow.
        if (result.Job == null || result.SkipReason is not SkipReason.None and not SkipReason.Failed)
        {
            return false;
        }

        log?.LogInformation("Adding rule job {jobId} for Rule(action={ruleAction}, trigger={ruleTrigger})",
            result.Job.Id,
            result.Rule.Action.GetType().Name,
            result.Rule.Trigger.GetType().Name);

        if (result.EnrichmentError != null || result.SkipReason is SkipReason.Failed)
        {
            writes.Add(new RuleEventWrite(result.Job, Error: result.EnrichmentError));
        }
        else
        {
            writes.Add(new RuleEventWrite(result.Job, result.Job.Created));
        }

        if (writes.Count >= 100)
        {
            writes.Clear();
            await repository.EnqueueAsync(writes, default);
        }

        return true;
    }

    public async ValueTask DisposeAsync()
    {
        if (writes.Count > 0)
        {
            await repository.EnqueueAsync(writes);
        }
    }
}
