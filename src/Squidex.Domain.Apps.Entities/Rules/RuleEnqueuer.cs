// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public RuleEnqueuer(IAppProvider appProvider, IRuleEventRepository ruleEventRepository,
            RuleService ruleService)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));

            this.appProvider = appProvider;

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
                var rules = await appProvider.GetRulesAsync(appEvent.AppId.Id);

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