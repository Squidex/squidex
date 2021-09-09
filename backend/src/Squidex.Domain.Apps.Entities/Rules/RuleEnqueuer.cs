// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class RuleEnqueuer : IEventConsumer, IRuleEnqueuer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(10);
        private readonly IMemoryCache cache;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IRuleService ruleService;
        private readonly IAppProvider appProvider;
        private readonly ILocalCache localCache;

        public string Name
        {
            get => GetType().Name;
        }

        public RuleEnqueuer(IAppProvider appProvider, IMemoryCache cache, ILocalCache localCache,
            IRuleEventRepository ruleEventRepository,
            IRuleService ruleService)
        {
            this.appProvider = appProvider;

            this.cache = cache;
            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;
            this.localCache = localCache;
        }

        public async Task EnqueueAsync(Rule rule, DomainId ruleId, Envelope<IEvent> @event)
        {
            Guard.NotNull(rule, nameof(rule));
            Guard.NotNull(@event, nameof(@event));

            var ruleContext = new RuleContext
            {
                Rule = rule,
                RuleId = ruleId
            };

            var jobs = ruleService.CreateJobsAsync(@event, ruleContext);

            await foreach (var job in jobs)
            {
                if (job.Job != null && job.SkipReason == SkipReason.None)
                {
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
            var cacheKey = $"{typeof(RuleEnqueuer)}_Rules_{appId}";

            return cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return appProvider.GetRulesAsync(appId);
            });
        }
    }
}
