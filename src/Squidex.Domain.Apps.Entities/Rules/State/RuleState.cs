// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules.State
{
    [CollectionName("Rules")]
    public class RuleState : DomainObjectState<RuleState>, IRuleEntity
    {
        [JsonProperty]
        public NamedId<Guid> AppId { get; set; }

        [JsonProperty]
        public Rule RuleDef { get; set; }

        [JsonProperty]
        public bool IsDeleted { get; set; }

        protected void On(RuleCreated @event)
        {
            RuleDef = new Rule(@event.Trigger, @event.Action);

            AppId = @event.AppId;
        }

        protected void On(RuleUpdated @event)
        {
            if (@event.Trigger != null)
            {
                RuleDef = RuleDef.Update(@event.Trigger);
            }

            if (@event.Action != null)
            {
                RuleDef = RuleDef.Update(@event.Action);
            }
        }

        protected void On(RuleEnabled @event)
        {
            RuleDef = RuleDef.Enable();
        }

        protected void On(RuleDisabled @event)
        {
            RuleDef = RuleDef.Disable();
        }

        protected void On(RuleDeleted @event)
        {
            IsDeleted = true;
        }

        public RuleState Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload));
        }
    }
}
