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
using Squidex.Domain.Apps.Events;
using Squidex.Flows;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleEnqueuer(
    IMemoryCache cache,
    ILocalCache localCache,
    IAppProvider appProvider,
    IFlowManager<FlowEventContext> flowManager,
    IRuleService ruleService,
    IRuleUsageTracker ruleUsageTracker,
    IOptions<RulesOptions> options,
    ILogger<RuleEnqueuer> log)
    : IEventConsumer, IRuleEnqueuer
{
    private readonly TimeSpan cacheDuration = options.Value.RulesCacheDuration;
    private readonly int maxExtraEvents = options.Value.MaxEnrichedEvents;

    public int BatchSize
    {
        get => 200;
    }

    public string Name
    {
        get => GetType().Name;
    }

    public async Task EnqueueAsync(Rule rule, Envelope<IEvent> @event,
        CancellationToken ct = default)
    {
        Guard.NotNull(rule);
        Guard.NotNull(@event);

        if (@event.Payload is not AppEvent appEvent)
        {
            return;
        }

        var context = new RulesContext
        {
            AppId = appEvent.AppId,
            IncludeSkipped = false,
            IncludeStale = false,
            Rules = rule != null ? new Dictionary<DomainId, Rule>
            {
                [rule.Id] = rule,
            }.ToReadonlyDictionary() : [],
        };

        await using var batch = new RuleQueueWriter(flowManager, ruleUsageTracker, log);
        await foreach (var result in ruleService.CreateJobsAsync(@event, context, ct))
        {
            await batch.WriteAsync(appEvent.AppId.Id, result);
        }
    }

    public async Task On(IEnumerable<Envelope<IEvent>> events)
    {
        using (localCache.StartContext())
        {
            // Write in batches of 100 items for better performance. Dispose completes the last write.
            await using var batch = new RuleQueueWriter(flowManager, ruleUsageTracker, log);

            static NamedId<DomainId>? GetAppId(Envelope<IEvent> @event)
            {
                // Returns null for events that are not handled, so that they all end up in one group to skip.
                if (@event.Headers.Restored() || @event.Payload is not AppEvent appEvent)
                {
                    return null;
                }

                return appEvent.AppId;
            }

            // The events of a batch usually belong to the same app, so the rules are only resolved
            // and indexed once per app instead of once per event.
            foreach (var byApp in events.GroupBy(GetAppId))
            {
                if (byApp.Key == null)
                {
                    continue;
                }

                var rules = await GetRulesAsync(byApp.Key.Id);
                if (rules.Count == 0)
                {
                    continue;
                }

                var context = new RulesContext
                {
                    AppId = byApp.Key,
                    AllowExtraEvents = maxExtraEvents > 0,
                    IncludeSkipped = false,
                    IncludeStale = false,
                    Rules = rules.ToReadonlyDictionary(x => x.Id),
                    MaxEvents = maxExtraEvents,
                };

                foreach (var @event in byApp)
                {
                    await foreach (var result in ruleService.CreateJobsAsync(@event, context))
                    {
                        await batch.WriteAsync(byApp.Key.Id, result);
                    }
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
