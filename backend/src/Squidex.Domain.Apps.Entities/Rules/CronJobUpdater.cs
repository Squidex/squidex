// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Events;
using Squidex.Flows.CronJobs;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class CronJobUpdater(IFlowCronJobManager<CronJobContext> flowCronJobs, IRuleEnqueuer ruleEnqueuer)
    : IEventConsumer, IInitializable
{
    public StreamFilter EventsFilter => StreamFilter.Prefix("rule-");

    public Task InitializeAsync(
        CancellationToken ct)
    {
        flowCronJobs.Subscribe(HandleCronJobAsync);
        return Task.CompletedTask;
    }

    public async Task HandleCronJobAsync(CronJob<CronJobContext> job,
        CancellationToken ct)
    {
        var ctx = job.Context;

        var @event =
            Envelope.Create(
                new RuleCronJobTriggered { AppId = ctx.AppId, RuleId = ctx.RuleId, Value = ctx.Value });

        await ruleEnqueuer.EnqueueAsync(job.Context.RuleId, null, @event, ct);
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (@event.Payload is RuleCreated ruleCreated && ruleCreated.Trigger is CronJobTrigger cronJobTrigger)
        {
            await AddCronJobAsync(ruleCreated.AppId, ruleCreated.RuleId, cronJobTrigger, default);
        }
        else if (@event.Payload is RuleUpdated ruleUpdated && ruleUpdated.Trigger is CronJobTrigger cronJobTrigger2)
        {
            await AddCronJobAsync(ruleUpdated.AppId, ruleUpdated.RuleId, cronJobTrigger2, default);
        }
        else if (@event.Payload is RuleDeleted ruleDeleted)
        {
            await flowCronJobs.RemoveAsync(ruleDeleted.RuleId.ToString());
        }
    }

    private async Task AddCronJobAsync(NamedId<DomainId> appId, DomainId id, CronJobTrigger trigger,
        CancellationToken ct)
    {
        await flowCronJobs.AddAsync(new CronJob<CronJobContext>
        {
            Id = trigger.ToString(),
            Context = new CronJobContext { AppId = appId, RuleId = id, Value = trigger.Value },
            CronExpression = trigger.CronExpression,
            CronTimezone = trigger.CronTimezone
        }, ct);
    }
}
