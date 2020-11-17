// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules.State
{
    [CollectionName("Rules")]
    public sealed class RuleState : DomainObjectState<RuleState>, IRuleEntity
    {
        public NamedId<DomainId> AppId { get; set; }

        public Rule RuleDef { get; set; }

        [IgnoreDataMember]
        public DomainId UniqueId
        {
            get { return DomainId.Combine(AppId, Id); }
        }

        public override bool ApplyEvent(IEvent @event)
        {
            var previousRule = RuleDef;

            switch (@event)
            {
                case RuleCreated e:
                    {
                        Id = e.RuleId;

                        RuleDef = new Rule(e.Trigger, e.Action);
                        RuleDef = RuleDef.Rename(e.Name);

                        AppId = e.AppId;

                        return true;
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

                        return true;
                    }
            }

            return !ReferenceEquals(previousRule, RuleDef);
        }
    }
}
