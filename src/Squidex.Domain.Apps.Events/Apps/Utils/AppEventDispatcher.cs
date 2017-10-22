// ==========================================================================
//  MongoAppRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Events.Apps.Utils
{
    public static class AppEventDispatcher
    {
        public static void Apply(this AppContributors contributors, AppContributorRemoved @event)
        {
            contributors.Remove(@event.ContributorId);
        }

        public static void Apply(this AppContributors contributors, AppContributorAssigned @event)
        {
            contributors.Assign(@event.ContributorId, @event.Permission);
        }

        public static void Apply(this LanguagesConfig languagesConfig, AppLanguageAdded @event)
        {
            languagesConfig.Set(new LanguageConfig(@event.Language));
        }

        public static void Apply(this LanguagesConfig languagesConfig, AppLanguageRemoved @event)
        {
            languagesConfig.Remove(@event.Language);
        }

        public static void Apply(this AppClients clients, AppClientAttached @event)
        {
            clients.Add(@event.Id, @event.Secret);
        }

        public static void Apply(this AppClients clients, AppClientRevoked @event)
        {
            clients.Revoke(@event.Id);
        }

        public static void Apply(this AppClients clients, AppClientRenamed @event)
        {
            if (clients.TryGetValue(@event.Id, out var client))
            {
                client.Rename(@event.Name);
            }
        }

        public static void Apply(this AppClients clients, AppClientUpdated @event)
        {
            if (clients.TryGetValue(@event.Id, out var client))
            {
                client.Update(@event.Permission);
            }
        }

        public static void Apply(this LanguagesConfig languagesConfig, AppLanguageUpdated @event)
        {
            languagesConfig.Set(new LanguageConfig(@event.Language, @event.IsOptional, @event.Fallback));

            if (@event.IsMaster)
            {
                languagesConfig.MakeMaster(@event.Language);
            }
        }
    }
}
