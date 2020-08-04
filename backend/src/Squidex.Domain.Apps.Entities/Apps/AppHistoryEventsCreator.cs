// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public AppHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<AppContributorAssigned>(
                T.Get("history.apps.contributoreAssigned"));

            AddEventMessage<AppContributorRemoved>(
                T.Get("history.apps.contributoreRemoved"));

            AddEventMessage<AppClientAttached>(
                T.Get("history.apps.clientAdded"));

            AddEventMessage<AppClientRevoked>(
                T.Get("history.apps.clientRevoked"));

            AddEventMessage<AppClientUpdated>(
                T.Get("history.apps.clientUpdated"));

            AddEventMessage<AppPlanChanged>(
                T.Get("history.apps.planChanged"));

            AddEventMessage<AppPlanReset>(
                T.Get("history.apps.planReset"));

            AddEventMessage<AppLanguageAdded>(
                T.Get("history.apps.languagedAdded"));

            AddEventMessage<AppLanguageRemoved>(
                T.Get("history.apps.languagedRemoved"));

            AddEventMessage<AppLanguageUpdated>(
                T.Get("history.apps.languagedUpdated"));

            AddEventMessage<AppMasterLanguageSet>(
                T.Get("history.apps.languagedSetToMaster"));

            AddEventMessage<AppPatternAdded>(
                T.Get("history.apps.patternAdded"));

            AddEventMessage<AppPatternDeleted>(
                T.Get("history.apps.patternDeleted"));

            AddEventMessage<AppPatternUpdated>(
                T.Get("history.apps.patternUpdated"));

            AddEventMessage<AppRoleAdded>(
                T.Get("history.apps.roleAdded"));

            AddEventMessage<AppRoleDeleted>(
                T.Get("history.apps.roleDeleted"));

            AddEventMessage<AppRoleUpdated>(
                T.Get("history.apps.roleUpdated"));
        }

        private HistoryEvent? CreateEvent(IEvent @event)
        {
            switch (@event)
            {
                case AppContributorAssigned e:
                    return CreateContributorsEvent(e, e.ContributorId, e.Role);
                case AppContributorRemoved e:
                    return CreateContributorsEvent(e, e.ContributorId);
                case AppClientAttached e:
                    return CreateClientsEvent(e, e.Id);
                case AppClientUpdated e:
                    return CreateClientsEvent(e, e.Id);
                case AppClientRevoked e:
                    return CreateClientsEvent(e, e.Id);
                case AppLanguageAdded e:
                    return CreateLanguagesEvent(e, e.Language);
                case AppLanguageUpdated e:
                    return CreateLanguagesEvent(e, e.Language);
                case AppMasterLanguageSet e:
                    return CreateLanguagesEvent(e, e.Language);
                case AppLanguageRemoved e:
                    return CreateLanguagesEvent(e, e.Language);
                case AppPatternAdded e:
                    return CreatePatternsEvent(e, e.PatternId, e.Name);
                case AppPatternUpdated e:
                    return CreatePatternsEvent(e, e.PatternId, e.Name);
                case AppPatternDeleted e:
                    return CreatePatternsEvent(e, e.PatternId);
                case AppRoleAdded e:
                    return CreateRolesEvent(e, e.Name);
                case AppRoleUpdated e:
                    return CreateRolesEvent(e, e.Name);
                case AppRoleDeleted e:
                    return CreateRolesEvent(e, e.Name);
                case AppPlanChanged e:
                    return CreatePlansEvent(e, e.PlanId);
                case AppPlanReset e:
                    return CreatePlansEvent(e);
            }

            return null;
        }

        private HistoryEvent CreateContributorsEvent(IEvent e, string contributor, string? role = null)
        {
            return ForEvent(e, "settings.contributors").Param("Contributor", contributor).Param("Role", role);
        }

        private HistoryEvent CreateLanguagesEvent(IEvent e, Language language)
        {
            return ForEvent(e, "settings.languages").Param("Language", language);
        }

        private HistoryEvent CreateRolesEvent(IEvent e, string name)
        {
            return ForEvent(e, "settings.roles").Param("Name", name);
        }

        private HistoryEvent CreatePatternsEvent(IEvent e, Guid id, string? name = null)
        {
            return ForEvent(e, "settings.patterns").Param("PatternId", id).Param("Name", name);
        }

        private HistoryEvent CreateClientsEvent(IEvent e, string id)
        {
            return ForEvent(e, "settings.clients").Param("Id", id);
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