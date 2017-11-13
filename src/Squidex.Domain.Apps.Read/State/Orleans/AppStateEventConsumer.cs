// ==========================================================================
//  StateEventConsumer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.State.Orleans.Grains;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.State.Orleans
{
    public sealed class AppStateEventConsumer : IEventConsumer
    {
        private readonly IGrainFactory factory;

        public string Name
        {
            get { return typeof(AppStateEventConsumer).Name; }
        }

        public string EventsFilter
        {
            get { return @"(^app-)|(^schema-)|(^rule\-)"; }
        }

        public AppStateEventConsumer(IGrainFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is AppEvent appEvent)
            {
                var appGrain = factory.GetGrain<IAppStateGrain>(appEvent.AppId.Name);

                await appGrain.HandleAsync(@event);
            }

            if (@event.Payload is AppContributorAssigned contributorAssigned)
            {
                var userGrain = factory.GetGrain<IAppUserGrain>(contributorAssigned.ContributorId);

                await userGrain.AddAppAsync(contributorAssigned.AppId.Name);
            }

            if (@event.Payload is AppContributorRemoved contributorRemoved)
            {
                var userGrain = factory.GetGrain<IAppUserGrain>(contributorRemoved.ContributorId);

                await userGrain.RemoveAppAsync(contributorRemoved.AppId.Name);
            }
        }
    }
}
