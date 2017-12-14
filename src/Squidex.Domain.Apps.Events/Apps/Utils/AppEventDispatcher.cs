// ==========================================================================
//  AppEventDispatcher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Events.Apps.Utils
{
    public static class AppEventDispatcher
    {
        public static AppContributors Apply(this AppContributors contributors, AppContributorRemoved @event)
        {
            return contributors.Remove(@event.ContributorId);
        }

        public static AppContributors Apply(this AppContributors contributors, AppContributorAssigned @event)
        {
            return contributors.Assign(@event.ContributorId, @event.Permission);
        }

        public static LanguagesConfig Apply(this LanguagesConfig languagesConfig, AppLanguageAdded @event)
        {
            return languagesConfig.Set(new LanguageConfig(@event.Language));
        }

        public static LanguagesConfig Apply(this LanguagesConfig languagesConfig, AppLanguageRemoved @event)
        {
            return languagesConfig.Remove(@event.Language);
        }

        public static AppClients Apply(this AppClients clients, AppClientAttached @event)
        {
            return clients.Add(@event.Id, @event.Secret);
        }

        public static AppClients Apply(this AppClients clients, AppClientRevoked @event)
        {
            return clients.Revoke(@event.Id);
        }

        public static AppClients Apply(this AppClients clients, AppClientRenamed @event)
        {
            return clients.Rename(@event.Id, @event.Name);
        }

        public static AppClients Apply(this AppClients clients, AppClientUpdated @event)
        {
            return clients.Update(@event.Id, @event.Permission);
        }

        public static LanguagesConfig Apply(this LanguagesConfig languagesConfig, AppLanguageUpdated @event)
        {
            var fallback = @event.Fallback;

            if (fallback != null && fallback.Count > 0)
            {
                var existingLangauges = languagesConfig.OfType<LanguageConfig>().Select(x => x.Language);

                fallback = fallback.Intersect(existingLangauges).ToList();
            }

            languagesConfig = languagesConfig.Set(new LanguageConfig(@event.Language, @event.IsOptional, fallback));

            if (@event.IsMaster)
            {
                languagesConfig = languagesConfig.MakeMaster(@event.Language);
            }

            return languagesConfig;
        }

        public static AppPatterns Apply(this AppPatterns patterns, AppPatternAdded @event)
        {
            return patterns.Add(@event.Id, @event.Name, @event.Pattern, @event.DefaultMessage);
        }

        public static AppPatterns Apply(this AppPatterns patterns, AppPatternDeleted @event)
        {
            return patterns.Remove(@event.Id);
        }

        public static AppPatterns Apply(this AppPatterns patterns, AppPatternUpdated @event)
        {
            return patterns.Update(@event.Id, @event.Name, @event.Pattern, @event.DefaultMessage);
        }
    }
}
