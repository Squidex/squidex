// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject;

public partial class RuleDomainObject
{
    protected override Rule Apply(Rule snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        switch (@event.Payload)
        {
            case RuleCreated e:
                newSnapshot = new Rule { Id = e.RuleId };
                SimpleMapper.Map(e, newSnapshot);
                break;

            case RuleUpdated e:
                {
                    if (e.Trigger != null)
                    {
                        newSnapshot = newSnapshot.Update(e.Trigger);
                    }

                    if (e.Action != null)
                    {
                        newSnapshot = newSnapshot.Update(e.Action);
                    }

                    if (e.Name != null)
                    {
                        newSnapshot = newSnapshot.Rename(e.Name);
                    }

                    if (e.IsEnabled == true)
                    {
                        newSnapshot = newSnapshot.Enable();
                    }
                    else if (e.IsEnabled == false)
                    {
                        newSnapshot = newSnapshot.Disable();
                    }

                    break;
                }

            case RuleEnabled:
                newSnapshot = newSnapshot.Enable();
                break;

            case RuleDisabled:
                newSnapshot = newSnapshot.Disable();
                break;

            case RuleDeleted:
                newSnapshot = newSnapshot with { IsDeleted = true };
                break;
        }

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}
