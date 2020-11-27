// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
            get { return GetType().Name; }
        }

        public RuleEnqueuer(IAppProvider appProvider, IMemoryCache cache, ILocalCache localCache,
            IRuleEventRepository ruleEventRepository,
            IRuleService ruleService)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(localCache, nameof(localCache));
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));

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

            var jobs = await ruleService.CreateJobsAsync(rule, ruleId, @event);

            foreach (var (job, ex) in jobs)
            {
                await ruleEventRepository.EnqueueAsync(job, ex);
            }
        }

        public async Task On(Envelope<IEvent> @event)
        {
            using (localCache.StartContext())
            {
                if (@event.Payload is AppEvent appEvent)
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
            return cache.GetOrCreateAsync(appId, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return appProvider.GetRulesAsync(appId);
            });
        }
    }
}