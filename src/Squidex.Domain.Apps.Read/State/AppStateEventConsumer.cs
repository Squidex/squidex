// ==========================================================================
//  AppStateEventConsumer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Read.State.Grains;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.State
{
    public sealed class AppStateEventConsumer : IEventConsumer
    {
        private readonly IStateFactory factory;

        public string Name
        {
            get { return typeof(AppStateEventConsumer).Name; }
        }

        public string EventsFilter
        {
            get { return @"(^app-)|(^schema-)|(^rule\-)"; }
        }

        public AppStateEventConsumer(IStateFactory factory)
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
                var appGrain = await factory.GetSynchronizedAsync<AppStateGrain>(appEvent.AppId.Name);

                await appGrain.HandleAsync(@event);
            }

            if (@event.Payload is AppContributorAssigned contributorAssigned)
            {
                var userGrain = await factory.GetSynchronizedAsync<AppUserGrain>(contributorAssigned.ContributorId);

                await userGrain.AddAppAsync(contributorAssigned.AppId.Name);
            }

            if (@event.Payload is AppContributorRemoved contributorRemoved)
            {
                var userGrain = await factory.GetSynchronizedAsync<AppUserGrain>(contributorRemoved.ContributorId);

                await userGrain.RemoveAppAsync(contributorRemoved.AppId.Name);
            }
        }
    }
}
