// ==========================================================================
//  AppStateGrainState_Rules.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Domain.Apps.Events.Rules.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Read.State.Grains
{
    public sealed partial class AppStateGrainState
    {
        public void On(RuleCreated @event, EnvelopeHeaders headers)
        {
            var id = @event.RuleId;

            Rules = Rules.SetItem(id, EntityMapper.Create<JsonRuleEntity>(@event, headers, r =>
            {
                r.RuleDef = RuleEventDispatcher.Create(@event);
            }));
        }

        public void On(RuleUpdated @event, EnvelopeHeaders headers)
        {
            UpdateRule(@event, headers, r =>
            {
                r.RuleDef = r.RuleDef.Apply(@event);
            });
        }

        public void On(RuleEnabled @event, EnvelopeHeaders headers)
        {
            UpdateRule(@event, headers, r =>
            {
                r.RuleDef = r.RuleDef.Apply(@event);
            });
        }

        public void On(RuleDisabled @event, EnvelopeHeaders headers)
        {
            UpdateRule(@event, headers, r =>
            {
                r.RuleDef = r.RuleDef.Apply(@event);
            });
        }

        public void On(RuleDeleted @event, EnvelopeHeaders headers)
        {
            Rules = Rules.Remove(@event.RuleId);
        }

        private void UpdateRule(RuleEvent @event, EnvelopeHeaders headers, Action<JsonRuleEntity> updater = null)
        {
            var id = @event.RuleId;

            Rules = Rules.SetItem(id, x => x.Clone().Update(@event, headers, updater));
        }
    }
}
