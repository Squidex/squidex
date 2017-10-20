// ==========================================================================
//  MongoAppRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.MongoDb.Apps
{
    public partial class MongoAppRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^app-"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected Task On(AppCreated @event, EnvelopeHeaders headers)
        {
            return Collection.CreateAsync(@event, headers, a =>
            {
                a.Clients = new Dictionary<string, MongoAppEntityClient>();
                a.Contributors = new Dictionary<string, MongoAppEntityContributor>();
                a.ContributorIds = new List<string>();

                SimpleMapper.Map(@event, a);
            });
        }

        protected Task On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients[@event.Id] = SimpleMapper.Map(@event, new MongoAppEntityClient());
            });
        }

        protected Task On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients.Remove(@event.Id);
            });
        }

        protected Task On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients[@event.Id].Name = @event.Name;
            });
        }

        protected Task On(AppClientUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients[@event.Id].Permission = @event.Permission;
            });
        }

        protected Task On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Contributors.Remove(@event.ContributorId);
            });
        }

        protected Task On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Contributors[@event.ContributorId] = new MongoAppEntityContributor { Permission = @event.Permission };
            });
        }

        protected Task On(AppLanguageAdded @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.UpdateLanguages(c => c.Add(@event.Language));
            });
        }

        protected Task On(AppLanguageRemoved @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.UpdateLanguages(c => c.Remove(@event.Language));
            });
        }

        protected Task On(AppLanguageUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.UpdateLanguages(c => c.Update(@event.Language, @event.IsOptional, @event.IsMaster, @event.Fallback));
            });
        }

        protected Task On(AppPlanChanged @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.ChangePlan(@event.PlanId, @event.Actor);
            });
        }

        private async Task UpdateAppAsync(AppEvent @event, EnvelopeHeaders headers, Action<MongoAppEntity> updater)
        {
            await Collection.UpdateAsync(@event, headers, a =>
            {
                updater(a);

                a.ContributorIds = a.Contributors.Keys.ToList();
            });
        }
    }
}
