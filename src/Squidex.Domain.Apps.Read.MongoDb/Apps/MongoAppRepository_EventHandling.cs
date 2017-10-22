// ==========================================================================
//  MongoAppRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Apps.Utils;
using Squidex.Domain.Apps.Read.MongoDb.Utils;
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
                SimpleMapper.Map(@event, a);
            });
        }

        protected Task On(AppPlanChanged @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                SimpleMapper.Map(@event, a);
            });
        }

        protected Task On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients.Apply(@event);
            });
        }

        protected Task On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients.Apply(@event);
            });
        }

        protected Task On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients.Apply(@event);
            });
        }

        protected Task On(AppClientUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Clients.Apply(@event);
            });
        }

        protected Task On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Contributors.Apply(@event);
            });
        }

        protected Task On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.Contributors.Apply(@event);
            });
        }

        protected Task On(AppLanguageAdded @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.LanguagesConfig.Apply(@event);
            });
        }

        protected Task On(AppLanguageRemoved @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.LanguagesConfig.Apply(@event);
            });
        }

        protected Task On(AppLanguageUpdated @event, EnvelopeHeaders headers)
        {
            return UpdateAppAsync(@event, headers, a =>
            {
                a.LanguagesConfig.Apply(@event);
            });
        }

        private async Task UpdateAppAsync(AppEvent @event, EnvelopeHeaders headers, Action<MongoAppEntity> updater)
        {
            await Collection.UpdateAsync(@event, headers, a =>
            {
                updater(a);

                a.ContributorIds = a.Contributors.Keys.ToArray();
            });
        }
    }
}
