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
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleEnqueuer : IEventConsumer, IRuleEnqueuer
{
    private readonly IMemoryCache cache;
    private readonly IRuleEventRepository ruleEventRepository;
    private readonly IRuleUsageTracker ruleUsageTracker;
    private readonly IRuleService ruleService;
    private readonly ILogger<RuleEnqueuer> log;
    private readonly IAppProvider appProvider;
    private readonly ILocalCache localCache;
    private readonly TimeSpan cacheDuration;
    private readonly int maxExtraEvents;

    public int BatchSize
    {
        get => 200;
    }

    public string Name
    {
        get => GetType().Name;
    }

    public RuleEnqueuer(IMemoryCache cache, ILocalCache localCache,
        IAppProvider appProvider,
        IRuleEventRepository ruleEventRepository,
        IRuleService ruleService,
        IRuleUsageTracker ruleUsageTracker,
        IOptions<RuleOptions> options,
        ILogger<RuleEnqueuer> log)
    {
        this.appProvider = appProvider;
        this.cache = cache;
        this.cacheDuration = options.Value.RulesCacheDuration;
        this.ruleEventRepository = ruleEventRepository;
        this.ruleUsageTracker = ruleUsageTracker;
        this.ruleService = ruleService;
        this.log = log;
        this.localCache = localCache;
        this.maxExtraEvents = options.Value.MaxEnrichedEvents;
    }

    public async Task EnqueueAsync(DomainId ruleId, Rule rule, Envelope<IEvent> @event)
    {
        Guard.NotNull(rule);
        Guard.NotNull(@event, nameof(@event));

        if (@event.Payload is not AppEvent appEvent)
        {
            return;
        }

        var context = new RulesContext
        {
            AppId = appEvent.AppId,
            IncludeSkipped = false,
            IncludeStale = false,
            Rules = new Dictionary<DomainId, Rule>
            {
                [ruleId] = rule
            }.ToReadonlyDictionary()
        };

        // Write in batches of 100 items for better performance. Dispose completes the last write.
        await using var batch = new RuleQueueWriter(ruleEventRepository, ruleUsageTracker, log);

        await foreach (var result in ruleService.CreateJobsAsync(@event, context))
        {
            await batch.WriteAsync(result);
        }
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        using (localCache.StartContext())
        {
            // Write in batches of 100 items for better performance. Dispose completes the last write.
            await using var batch = new RuleQueueWriter(ruleEventRepository, ruleUsageTracker, log);

            foreach (var @event in events)
            {
                if (@event.Headers.Restored())
                {
                    continue;
                }

                if (@event.Payload is not AppEvent appEvent)
                {
                    continue;
                }

                var rules = await GetRulesAsync(appEvent.AppId.Id);

                var context = new RulesContext
                {
                    AppId = appEvent.AppId,
                    AllowExtraEvents = maxExtraEvents > 0,
                    IncludeSkipped = false,
                    IncludeStale = false,
                    Rules = rules.ToReadonlyDictionary(x => x.Id),
                    MaxEvents = maxExtraEvents
                };

                await foreach (var result in ruleService.CreateJobsAsync(@event, context))
                {
                    await batch.WriteAsync(result);
                }
            }
        }
    }

    private Task<List<Rule>> GetRulesAsync(DomainId appId)
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
