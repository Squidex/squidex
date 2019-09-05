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

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public AppHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<AppContributorAssigned>(
                "assigned {user:[Contributor]} as {[Role]}");

            AddEventMessage<AppContributorRemoved>(
                "removed {user:[Contributor]} from app");

            AddEventMessage<AppClientAttached>(
                "added client {[Id]} to app");

            AddEventMessage<AppClientRevoked>(
                "revoked client {[Id]}");

            AddEventMessage<AppClientUpdated>(
                "updated client {[Id]}");

            AddEventMessage<AppClientRenamed>(
                "renamed client {[Id]} to {[Name]}");

            AddEventMessage<AppPlanChanged>(
                "changed plan to {[Plan]}");

            AddEventMessage<AppPlanReset>(
                "resetted plan");

            AddEventMessage<AppLanguageAdded>(
                "added language {[Language]}");

            AddEventMessage<AppLanguageRemoved>(
                "removed language {[Language]}");

            AddEventMessage<AppLanguageUpdated>(
                "updated language {[Language]}");

            AddEventMessage<AppMasterLanguageSet>(
                "changed master language to {[Language]}");

            AddEventMessage<AppPatternAdded>(
                "added pattern {[Name]}");

            AddEventMessage<AppPatternDeleted>(
                "deleted pattern {[PatternId]}");

            AddEventMessage<AppPatternUpdated>(
                "updated pattern {[Name]}");

            AddEventMessage<AppRoleAdded>(
                "added role {[Name]}");

            AddEventMessage<AppRoleDeleted>(
                "deleted role {[Name]}");

            AddEventMessage<AppRoleUpdated>(
                "updated role {[Name]}");
        }

        private HistoryEvent CreateEvent(IEvent @event)
        {
            switch (@event)
            {
                case AppContributorAssigned e:
                    return CreateContributorsEvent(e, e.ContributorId, e.Role);
                case AppContributorRemoved e:
                    return CreateContributorsEvent(e, e.ContributorId);
                case AppClientAttached e:
                    return CreateClientsEvent(e, e.Id);
                case AppClientRenamed e:
                    return CreateClientsEvent(e, e.Id, ClientName(e));
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

        private HistoryEvent CreateContributorsEvent(IEvent e, string contributor, string role = null)
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

        private HistoryEvent CreatePatternsEvent(IEvent e, Guid id, string name = null)
        {
            return ForEvent(e, "settings.patterns").Param("PatternId", id).Param("Name", name);
        }

        private HistoryEvent CreateClientsEvent(IEvent e, string id, string name = null)
        {
            return ForEvent(e, "settings.clients").Param("Id", id).Param("Name", name);
        }

        private HistoryEvent CreatePlansEvent(IEvent e, string plan = null)
        {
            return ForEvent(e, "settings.plan").Param("Plan", plan);
        }

        protected override Task<HistoryEvent> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            return Task.FromResult(CreateEvent(@event.Payload));
        }

        private static string ClientName(AppClientRenamed e)
        {
            return !string.IsNullOrWhiteSpace(e.Name) ? e.Name : e.Id;
        }
    }
}