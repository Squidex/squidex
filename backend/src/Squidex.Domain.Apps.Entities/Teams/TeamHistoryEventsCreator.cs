// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events.Teams;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Teams.Entities.Teams
{
    public sealed class TeamHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public TeamHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<TeamContributorAssigned>(
                "history.teams.contributoreAssigned");

            AddEventMessage<TeamContributorRemoved>(
                "history.teams.contributoreRemoved");

            AddEventMessage<TeamPlanChanged>(
                "history.teams.planChanged");

            AddEventMessage<TeamPlanReset>(
                "history.teams.planReset");

            AddEventMessage<TeamPlanReset>(
                "history.teams.updated");
        }

        private HistoryEvent? CreateEvent(IEvent @event)
        {
            switch (@event)
            {
                case TeamContributorAssigned e:
                    return CreateContributorsEvent(e, e.ContributorId, e.Role);
                case TeamContributorRemoved e:
                    return CreateContributorsEvent(e, e.ContributorId);
                case TeamPlanChanged e:
                    return CreatePlansEvent(e, e.PlanId);
                case TeamPlanReset e:
                    return CreatePlansEvent(e);
                case TeamUpdated e:
                    return CreateGeneralEvent(e);
            }

            return null;
        }

        private HistoryEvent CreateGeneralEvent(IEvent e)
        {
            return ForEvent(e, "settings.general");
        }

        private HistoryEvent CreateContributorsEvent(IEvent e, string contributor, string? role = null)
        {
            return ForEvent(e, "settings.contributors").Param("Contributor", contributor).Param("Role", role);
        }

        private HistoryEvent CreatePlansEvent(IEvent e, string? plan = null)
        {
            return ForEvent(e, "settings.plan").Param("Plan", plan);
        }

        protected override Task<HistoryEvent?> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            return Task.FromResult(CreateEvent(@event.Payload));
        }
    }
}
