// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Teams.DomainObject;

public partial class TeamDomainObject
{
    public sealed class State : DomainObjectState<State>, ITeamEntity
    {
        public string Name { get; set; }

        public Contributors Contributors { get; set; } = Contributors.Empty;

        public AssignedPlan? Plan { get; set; }

        [JsonIgnore]
        public DomainId UniqueId
        {
            get => Id;
        }

        public override bool ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case TeamCreated e:
                    {
                        Id = e.TeamId;

                        SimpleMapper.Map(e, this);
                        return true;
                    }

                case TeamUpdated e when Is.Change(Name, e.Name):
                    {
                        SimpleMapper.Map(e, this);
                        return true;
                    }

                case TeamPlanChanged e when Is.Change(Plan?.PlanId, e.PlanId):
                    return UpdatePlan(e.ToPlan());

                case TeamPlanReset e when Plan != null:
                    return UpdatePlan(null);

                case TeamContributorAssigned e:
                    return UpdateContributors(e, (e, c) => c.Assign(e.ContributorId, e.Role));

                case TeamContributorRemoved e:
                    return UpdateContributors(e, (e, c) => c.Remove(e.ContributorId));
            }

            return false;
        }

        private bool UpdateContributors<T>(T @event, Func<T, Contributors, Contributors> update)
        {
            var previous = Contributors;

            Contributors = update(@event, previous);

            return !ReferenceEquals(previous, Contributors);
        }

        private bool UpdatePlan(AssignedPlan? plan)
        {
            Plan = plan;

            return true;
        }
    }
}
