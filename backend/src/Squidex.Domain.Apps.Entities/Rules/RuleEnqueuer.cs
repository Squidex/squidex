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

        // Write in batches of 100 items for better performance. Using completes the last write.
        await using var batch = new RuleQueueWriter(ruleEventRepository);

        await ruleService.CreateJobsAsync(async (ruleId, rule, result, ct) =>
        {
            await batch.WriteAsync(result, rule, log);
        }, @event, context, default);
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        using (localCache.StartContext())
        {
            // Write in batches of 100 items for better performance. Using completes the last write.
            await using var batch = new RuleQueueWriter(ruleEventRepository);

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
                    IncludeSkipped = false,
                    IncludeStale = false,
                    Rules = rules.ToDictionary(x => x.Id, x => x.RuleDef).ToReadonlyDictionary(),
                };

                await ruleService.CreateJobsAsync(async (ruleId, rule, result, ct) =>
                {
                    await batch.WriteAsync(result, rule, log);
                }, @event, context, default);
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
