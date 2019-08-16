// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;

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

        protected Task<HistoryEvent> On(AppContributorRemoved @event)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId));
        }

        protected Task<HistoryEvent> On(AppContributorAssigned @event)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId).AddParameter("Role", @event.Role));
        }

        protected Task<HistoryEvent> On(AppClientAttached @event)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEvent> On(AppClientRevoked @event)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEvent> On(AppClientRenamed @event)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id).AddParameter("Name", ClientName(@event)));
        }

        protected Task<HistoryEvent> On(AppLanguageAdded @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEvent> On(AppLanguageRemoved @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEvent> On(AppLanguageUpdated @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEvent> On(AppMasterLanguageSet @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEvent> On(AppPatternAdded @event)
        {
            const string channel = "settings.patterns";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", @event.Name));
        }

        protected Task<HistoryEvent> On(AppPatternUpdated @event)
        {
            const string channel = "settings.patterns";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", @event.Name));
        }

        protected Task<HistoryEvent> On(AppPatternDeleted @event)
        {
            const string channel = "settings.patterns";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("PatternId", @event.PatternId));
        }

        protected Task<HistoryEvent> On(AppRoleAdded @event)
        {
            const string channel = "settings.roles";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", @event.Name));
        }

        protected Task<HistoryEvent> On(AppRoleUpdated @event)
        {
            const string channel = "settings.roles";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", @event.Name));
        }

        protected Task<HistoryEvent> On(AppRoleDeleted @event)
        {
            const string channel = "settings.roles";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", @event.Name));
        }

        protected Task<HistoryEvent> On(AppPlanChanged @event)
        {
            const string channel = "settings.plan";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Plan", @event.PlanId));
        }

        protected Task<HistoryEvent> On(AppPlanReset @event)
        {
            const string channel = "settings.plan";

            return Task.FromResult(
                ForEvent(@event, channel));
        }

        protected override Task<HistoryEvent> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            return this.DispatchFuncAsync(@event.Payload, (HistoryEvent)null);
        }

        private static string ClientName(AppClientRenamed @event)
        {
            return !string.IsNullOrWhiteSpace(@event.Name) ? @event.Name : @event.Id;
        }
    }
}