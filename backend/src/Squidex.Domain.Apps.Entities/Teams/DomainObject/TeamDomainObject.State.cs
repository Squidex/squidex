// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject;

public partial class TeamDomainObject
{
    protected override Team Apply(Team snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        switch (@event.Payload)
        {
            case TeamCreated e:
                newSnapshot = new Team { Id = e.TeamId, Name = e.Name };
                break;

            case TeamUpdated e:
                newSnapshot = snapshot.Rename(e.Name);
                break;

            case TeamPlanChanged e:
                newSnapshot = snapshot.ChangePlan(new AssignedPlan(e.Actor, e.PlanId));
                break;

            case TeamPlanReset e:
                newSnapshot = snapshot.ChangePlan(null);
                break;

            case TeamContributorAssigned e:
                newSnapshot = snapshot.UpdateContributors(e, (e, c) => c.Assign(e.ContributorId, e.Role));
                break;

            case TeamContributorRemoved e:
                newSnapshot = snapshot.UpdateContributors(e, (e, c) => c.Remove(e.ContributorId));
                break;

            case TeamAuthChanged e:
                newSnapshot = snapshot.ChangeAuthScheme(e.Scheme);
                break;

            case TeamDeleted:
                newSnapshot = snapshot with { Plan = null, IsDeleted = true };
                break;
        }

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}
