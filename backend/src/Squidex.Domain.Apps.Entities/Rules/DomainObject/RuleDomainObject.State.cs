// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject
{
    public sealed partial class RuleDomainObject
    {
        [CollectionName("Rules")]
        public sealed class State : DomainObjectState<State>, IRuleEntity
        {
            public NamedId<DomainId> AppId { get; set; }

            public Rule RuleDef { get; set; }

            public bool IsDeleted { get; set; }

            [IgnoreDataMember]
            public DomainId UniqueId
            {
                get => DomainId.Combine(AppId, Id);
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

                            if (e.IsEnabled == true)
                            {
                                RuleDef = RuleDef.Enable();
                            }
                            else if (e.IsEnabled == false)
                            {
                                RuleDef = RuleDef.Disable();
                            }

                            break;
                        }

                    case RuleEnabled:
                        {
                            RuleDef = RuleDef.Enable();

                            break;
                        }

                    case RuleDisabled:
                        {
                            RuleDef = RuleDef.Disable();

                            break;
                        }

                    case RuleDeleted:
                        {
                            IsDeleted = true;

                            return true;
                        }
                }

                return !ReferenceEquals(previousRule, RuleDef);
            }
        }
    }
}
