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
                "assigned {user:[Contributor]} as {[Permission]}");

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
        }

        protected Task<HistoryEventToStore> On(AppContributorRemoved @event)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId));
        }

        protected Task<HistoryEventToStore> On(AppContributorAssigned @event)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId).AddParameter("Permission", @event.Permission));
        }

        protected Task<HistoryEventToStore> On(AppClientAttached @event)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEventToStore> On(AppClientRevoked @event)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEventToStore> On(AppClientRenamed @event)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id).AddParameter("Name", ClientName(@event)));
        }

        protected Task<HistoryEventToStore> On(AppLanguageAdded @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEventToStore> On(AppLanguageRemoved @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEventToStore> On(AppLanguageUpdated @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEventToStore> On(AppMasterLanguageSet @event)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEventToStore> On(AppPatternAdded @event)
        {
            const string channel = "settings.patterns";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", @event.Name));
        }

        protected Task<HistoryEventToStore> On(AppPatternUpdated @event)
        {
            const string channel = "settings.patterns";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Name", @event.Name));
        }

        protected Task<HistoryEventToStore> On(AppPatternDeleted @event)
        {
            const string channel = "settings.patterns";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("PatternId", @event.PatternId));
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            return this.DispatchFuncAsync(@event.Payload, (HistoryEventToStore)null);
        }

        private static string ClientName(AppClientRenamed @event)
        {
            return !string.IsNullOrWhiteSpace(@event.Name) ? @event.Name : @event.Id;
        }
    }
}