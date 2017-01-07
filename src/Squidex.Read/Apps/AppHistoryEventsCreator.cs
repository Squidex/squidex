// ==========================================================================
//  AppHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Read.History;

namespace Squidex.Read.Apps
{
    public class AppHistoryEventsCreator : IHistoryEventsCreator
    {
        private static readonly IReadOnlyDictionary<string, string> TextsEN =
            new Dictionary<string, string>
            {
                {
                    TypeNameRegistry.GetName<AppContributorAssigned>(),
                    "assigned {user:[Contributor]} as [Permission]"
                },
                {
                    TypeNameRegistry.GetName<AppContributorRemoved>(),
                    "removed {user:[Contributor]} from app"
                },
                {
                    TypeNameRegistry.GetName<AppClientAttached>(),
                    "added client {[Id]} to app"
                },
                {
                    TypeNameRegistry.GetName<AppClientRevoked>(),
                    "revoked client {[Id]}"
                },
                {
                    TypeNameRegistry.GetName<AppClientRenamed>(),
                    "named client {[Id]} as {[Name]}"
                },
                {
                    TypeNameRegistry.GetName<AppLanguageAdded>(),
                    "added language {[Language]}"
                },
                {
                    TypeNameRegistry.GetName<AppLanguageRemoved>(),
                    "removed language {[Language]}"
                },
                {
                    TypeNameRegistry.GetName<AppMasterLanguageSet>(),
                    "changed master language to {[Language]}"
                }
            };

        public IReadOnlyDictionary<string, string> Texts
        {
            get { return TextsEN; }
        }

        protected Task<HistoryEventToStore> On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId)
                    .AddParameter("Permission", @event.Permission.ToString()));
        }

        protected Task<HistoryEventToStore> On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.contributors";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Contributor", @event.ContributorId));
        }

        protected Task<HistoryEventToStore> On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Id", @event.Id)
                    .AddParameter("Name", !string.IsNullOrWhiteSpace(@event.Name) ? @event.Name : @event.Id));
        }

        protected Task<HistoryEventToStore> On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEventToStore> On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.clients";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Id", @event.Id));
        }

        protected Task<HistoryEventToStore> On(AppLanguageAdded @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Language", @event.Language.EnglishName));
        }

        protected Task<HistoryEventToStore> On(AppLanguageRemoved @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Language", @event.Language.EnglishName));
        }

        protected Task<HistoryEventToStore> On(AppMasterLanguageSet @event, EnvelopeHeaders headers)
        {
            const string channel = "settings.languages";

            return Task.FromResult(
                HistoryEventToStore.Create(@event, channel)
                    .AddParameter("Language", @event.Language.EnglishName));
        }

        public Task<HistoryEventToStore> CreateEventAsync(Envelope<IEvent> @event)
        {
            return this.DispatchFuncAsync(@event.Payload, @event.Headers, (HistoryEventToStore)null);
        }
    }
}