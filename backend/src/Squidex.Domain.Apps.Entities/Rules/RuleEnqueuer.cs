// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleEnqueuer : IEventConsumer, IRuleEnqueuer
{
    private readonly IMemoryCache cache;
    private readonly IRuleEventRepository ruleEventRepository;
    private readonly IRuleService ruleService;
    private readonly ILogger<RuleEnqueuer> log;
    private readonly IAppProvider appProvider;
    private readonly ILocalCache localCache;
    private readonly TimeSpan cacheDuration;

    public string Name
    {
        get => GetType().Name;
    }

    public RuleEnqueuer(IMemoryCache cache, ILocalCache localCache,
        IAppProvider appProvider,
        IRuleEventRepository ruleEventRepository,
        IRuleService ruleService,
        IOptions<RuleOptions> options,
        ILogger<RuleEnqueuer> log)
    {
        this.appProvider = appProvider;
        this.cache = cache;
        this.cacheDuration = options.Value.RulesCacheDuration;
        this.ruleEventRepository = ruleEventRepository;
        this.ruleService = ruleService;
        this.log = log;
        this.localCache = localCache;
    }

    public async Task EnqueueAsync(Rule rule, DomainId ruleId, Envelope<IEvent> @event)
    {
        Guard.NotNull(rule);
        Guard.NotNull(@event, nameof(@event));

        var ruleContext = new RuleContext
        {
            Rule = rule,
            RuleId = ruleId
        };

        var jobs = ruleService.CreateJobsAsync(@event, ruleContext);

        await foreach (var job in jobs)
        {
            // We do not want to handle disabled rules in the normal flow.
            if (job.Job != null && job.SkipReason is SkipReason.None or SkipReason.Failed)
            {
                log.LogInformation("Adding rule job {jobId} for Rule(action={ruleAction}, trigger={ruleTrigger})", job.Job.Id,
                    rule.Action.GetType().Name, rule.Trigger.GetType().Name);

                await ruleEventRepository.EnqueueAsync(job.Job, job.EnrichmentError);
            }
        }
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (@event.Headers.Restored())
        {
            return;
        }

        if (@event.Payload is AppEvent appEvent)
        {
            using (localCache.StartContext())
            {
                var rules = await GetRulesAsync(appEvent.AppId.Id);

                foreach (var ruleEntity in rules)
                {
                    await EnqueueAsync(ruleEntity.RuleDef, ruleEntity.Id, @event);
                }
            }
        }
    }

    private Task<List<IRuleEntity>> GetRulesAsync(DomainId appId)
    {
        if (cacheDuration <= TimeSpan.Zero || cacheDuration == TimeSpan.MaxValue)
        {
            return appProvider.GetRulesAsync(appId);
        }

        var cacheKey = $"{typeof(RuleEnqueuer)}_Rules_{appId}";

        // Cache the rules for performance reasons for a short period of time (usually 10 sec).
        return cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = cacheDuration;

            return appProvider.GetRulesAsync(appId);
        })!;
    }
}
