// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    public sealed class RuleEventDto : Resource
    {
        /// <summary>
        /// The id of the event.
        /// </summary>
        public DomainId Id { get; set; }

        /// <summary>
        /// The time when the event has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        [LocalizedRequired]
        public string Description { get; set; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        [LocalizedRequired]
        public string EventName { get; set; }

        /// <summary>
        /// The last dump.
        /// </summary>
        public string? LastDump { get; set; }

        /// <summary>
        /// The number of calls.
        /// </summary>
        public int NumCalls { get; set; }

        /// <summary>
        /// The next attempt.
        /// </summary>
        public Instant? NextAttempt { get; set; }

        /// <summary>
        /// The result of the event.
        /// </summary>
        public RuleResult Result { get; set; }

        /// <summary>
        /// The result of the job.
        /// </summary>
        public RuleJobResult JobResult { get; set; }

        public static RuleEventDto FromRuleEvent(IRuleEventEntity ruleEvent, Resources resources)
        {
            var result = new RuleEventDto();

            SimpleMapper.Map(ruleEvent, result);
            SimpleMapper.Map(ruleEvent.Job, result);

            return result.CreateLinks(resources);
        }

        private RuleEventDto CreateLinks(Resources resources)
        {
            var values = new { app = resources.App, id = Id };

            AddPutLink("update", resources.Url<RulesController>(x => nameof(x.PutEvent), values));

            if (NextAttempt != null)
            {
                AddDeleteLink("cancel", resources.Url<RulesController>(x => nameof(x.DeleteEvent), values));
            }

            return this;
        }
    }
}
