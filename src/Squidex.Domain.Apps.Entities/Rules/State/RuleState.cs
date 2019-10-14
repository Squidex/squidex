// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Rules.State
{
    [CollectionName("Rules")]
    public class RuleState : DomainObjectState<RuleState>, IRuleEntity
    {
        [DataMember]
        public NamedId<Guid> AppId { get; set; }

        [DataMember]
        public Rule RuleDef { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        public void ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case RuleCreated e:
                    {
                        RuleDef = new Rule(e.Trigger, e.Action);
                        RuleDef = RuleDef.Rename(e.Name);

                        AppId = e.AppId;

                        break;
                    }

                case RuleUpdated e:
                    {
                        if (e.Trigger != null)
                        {
                            RuleDef = RuleDef.Update(e.Trigger);
                        }

                        if (e.Action != null)
                        {
                            RuleDef = RuleDef.Update(e.Action);
                        }

                        if (e.Name != null)
                        {
                            RuleDef = RuleDef.Rename(e.Name);
                        }

                        break;
                    }

                case RuleEnabled _:
                    {
                        RuleDef = RuleDef.Enable();

                        break;
                    }

                case RuleDisabled _:
                    {
                        RuleDef = RuleDef.Disable();

                        break;
                    }

                case RuleDeleted _:
                    {
                        IsDeleted = true;

                        break;
                    }
            }
        }

        public override RuleState Apply(Envelope<IEvent> @event)
        {
            return Clone().Update(@event, (e, s) => s.ApplyEvent(e));
        }
    }
}
