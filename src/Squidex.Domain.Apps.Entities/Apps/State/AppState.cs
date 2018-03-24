// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Apps.State
{
    public class AppState : DomainObjectState<AppState>,
        IAppEntity
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public AppPlan Plan { get; set; }

        [JsonProperty]
        public AppClients Clients { get; set; } = AppClients.Empty;

        [JsonProperty]
        public AppPatterns Patterns { get; set; } = AppPatterns.Empty;

        [JsonProperty]
        public AppContributors Contributors { get; set; } = AppContributors.Empty;

        [JsonProperty]
        public LanguagesConfig LanguagesConfig { get; set; } = LanguagesConfig.English;

        [JsonProperty]
        public bool IsArchived { get; set; }

        protected void On(AppCreated @event)
        {
            SimpleMapper.Map(@event, this);
        }

        protected void On(AppPlanChanged @event)
        {
            Plan = @event.PlanId == null ? null : new AppPlan(@event.Actor, @event.PlanId);
        }

        protected void On(AppContributorAssigned @event)
        {
            Contributors = Contributors.Assign(@event.ContributorId, @event.Permission);
        }

        protected void On(AppContributorRemoved @event)
        {
            Contributors = Contributors.Remove(@event.ContributorId);
        }

        protected void On(AppClientAttached @event)
        {
            Clients = Clients.Add(@event.Id, @event.Secret);
        }

        protected void On(AppClientUpdated @event)
        {
            Clients = Clients.Update(@event.Id, @event.Permission);
        }

        protected void On(AppClientRenamed @event)
        {
            Clients = Clients.Rename(@event.Id, @event.Name);
        }

        protected void On(AppClientRevoked @event)
        {
            Clients = Clients.Revoke(@event.Id);
        }

        protected void On(AppPatternAdded @event)
        {
            Patterns = Patterns.Add(@event.PatternId, @event.Name, @event.Pattern, @event.Message);
        }

        protected void On(AppPatternDeleted @event)
        {
            Patterns = Patterns.Remove(@event.PatternId);
        }

        protected void On(AppPatternUpdated @event)
        {
            Patterns = Patterns.Update(@event.PatternId, @event.Name, @event.Pattern, @event.Message);
        }

        protected void On(AppLanguageAdded @event)
        {
            LanguagesConfig = LanguagesConfig.Set(new LanguageConfig(@event.Language));
        }

        protected void On(AppLanguageRemoved @event)
        {
            LanguagesConfig = LanguagesConfig.Remove(@event.Language);
        }

        protected void On(AppLanguageUpdated @event)
        {
            LanguagesConfig = LanguagesConfig.Set(new LanguageConfig(@event.Language, @event.IsOptional, @event.Fallback));

            if (@event.IsMaster)
            {
                LanguagesConfig = LanguagesConfig.MakeMaster(@event.Language);
            }
        }

        protected void On(AppArchived @event)
        {
            Plan = null;

            IsArchived = true;
        }

        public AppState Apply(Envelope<IEvent> @event)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload));
        }
    }
}
