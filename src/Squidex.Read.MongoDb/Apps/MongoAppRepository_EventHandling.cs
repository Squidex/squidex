// ==========================================================================
//  MongoAppRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.MongoDb.Utils;

namespace Squidex.Read.MongoDb.Apps
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
                SimpleMapper.Map(@event, a);
            });
        }

        protected Task On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.Contributors.Remove(@event.ContributorId);
            });
        }

        protected Task On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.Clients[@event.Id] = SimpleMapper.Map(@event, new MongoAppClientEntity());
            });
        }

        protected Task On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.Clients.Remove(@event.Id);
            });
        }

        protected Task On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.Clients[@event.Id].Name = @event.Name;
            });
        }

        protected Task On(AppLanguageAdded @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.UpdateLanguages(c => c.Add(@event.Language));
            });
        }

        protected Task On(AppLanguageRemoved @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.UpdateLanguages(c => c.Remove(@event.Language));
            });
        }

        protected Task On(AppLanguageUpdated @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.UpdateLanguages(c => c.Update(@event.Language, @event.IsOptional, @event.IsMaster, @event.Fallback));
            });
        }

        protected Task On(AppPlanChanged @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                a.PlanOwner = @event.Actor.Identifier;
                a.PlanId = @event.PlanId;
            });
        }

        protected Task On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            return Collection.UpdateAsync(@event, headers, a =>
            {
                var contributor = a.Contributors.GetOrAddNew(@event.ContributorId);

                SimpleMapper.Map(@event, contributor);
            });
        }
    }
}
