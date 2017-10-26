// ==========================================================================
//  RuleEnqueuer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Rules
{
    public sealed class RuleEnqueuer : IEventConsumer
    {
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IRuleRepository ruleRepository;
        private readonly RuleService ruleService;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public RuleEnqueuer(
            IRuleEventRepository ruleEventRepository,
            IRuleRepository ruleRepository,
            RuleService ruleService)
        {
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleRepository, nameof(ruleRepository));
            Guard.NotNull(ruleService, nameof(ruleService));

            this.ruleEventRepository = ruleEventRepository;
            this.ruleRepository = ruleRepository;
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
                var rules = await ruleRepository.QueryCachedByAppAsync(appEvent.AppId.Id);

                foreach (var ruleEntity in rules)
                {
                    var job = ruleService.CreateJob(ruleEntity.Rule, @event);

                    if (job == null)
                    {
                        continue;
                    }

                    await ruleEventRepository.EnqueueAsync(job, job.Created);
                }
            }
        }
    }
}