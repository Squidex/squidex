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
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class RuleEnqueuer : IEventConsumer
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IAppProvider appProvider;
        private readonly IMemoryCache cache;
        private readonly RuleService ruleService;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return ".*"; }
        }

        public RuleEnqueuer(IAppProvider appProvider, IMemoryCache cache, IRuleEventRepository ruleEventRepository,
            RuleService ruleService)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));

            this.appProvider = appProvider;

            this.cache = cache;

            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is AppEvent appEvent)
            {
                var rules = await GetRulesAsync(appEvent.AppId.Id);

                foreach (var ruleEntity in rules)
                {
                    var job = await ruleService.CreateJobAsync(ruleEntity.RuleDef, @event);

                    if (job != null)
                    {
                        await ruleEventRepository.EnqueueAsync(job, job.Created);
                    }
                }
            }
        }

        private Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            return cache.GetOrCreateAsync(appId, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return appProvider.GetRulesAsync(appId);
            });
        }
    }
}