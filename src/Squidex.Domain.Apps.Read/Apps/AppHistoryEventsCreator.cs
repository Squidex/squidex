// ==========================================================================
//  AppHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Read.Apps
{
    public class AppHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public AppHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<AppContributorAssigned>(
                "assigned {user:[Contributor]} as [Permission]");

            AddEventMessage<AppContributorRemoved>(
                "removed {user:[Contributor]} from app");

            AddEventMessage<AppClientAttached>(
                "added client {[Id]} to app");

            AddEventMessage<AppClientRevoked>(
                "revoked client {[Id]}");

            AddEventMessage<AppClientRenamed>(
                "named client {[Id]} as {[Name]}");

            AddEventMessage<AppLanguageAdded>(
                "added language {[Language]}");

            AddEventMessage<AppLanguageRemoved>(
                "removed language {[Language]}");

            AddEventMessage<AppLanguageUpdated>(
                "updated language {[Language]}");

            AddEventMessage<AppMasterLanguageSet>(
                "changed master language to {[Language]}");
        }

        protected Task<HistoryEventToStore> On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId));
        }

        protected Task<HistoryEventToStore> On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId).AddParameter("Permission", @event.Permission));
        }

        protected Task<HistoryEventToStore> On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEventToStore> On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEventToStore> On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Id", @event.Id).AddParameter("Name", ClientName(@event)));
        }

        protected Task<HistoryEventToStore> On(AppLanguageAdded @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEventToStore> On(AppLanguageRemoved @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEventToStore> On(AppLanguageUpdated @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected Task<HistoryEventToStore> On(AppMasterLanguageSet @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                ForEvent(@event, channel)
                    .AddParameter("Language", @event.Language));
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            return this.DispatchFuncAsync(@event.Payload, @event.Headers, (HistoryEventToStore)null);
        }

        private static string ClientName(AppClientRenamed @event)
        {
            return !string.IsNullOrWhiteSpace(@event.Name) ? @event.Name : @event.Id;
        }
    }
}