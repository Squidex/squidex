// ==========================================================================
//  RuleEnqueuer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
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
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IAppProvider appProvider;
        private readonly RuleService ruleService;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return ".*"; }
        }

        public RuleEnqueuer(
            IRuleEventRepository ruleEventRepository, IAppProvider appProvider,
            RuleService ruleService)
        {
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));

            Guard.NotNull(appProvider, nameof(appProvider));

            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;

            this.appProvider = appProvider;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is AppEvent appEvent)
            {
                var rules = await appProvider.GetRulesAsync(appEvent.AppId.Name);

                foreach (var ruleEntity in rules)
                {
                    var job = ruleService.CreateJob(ruleEntity.RuleDef, @event);

                    if (job != null)
                    {
                        await ruleEventRepository.EnqueueAsync(job, job.Created);
                    }
                }
            }
        }
    }
}