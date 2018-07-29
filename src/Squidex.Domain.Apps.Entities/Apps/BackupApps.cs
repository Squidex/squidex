// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class BackupApps : BackupHandlerWithStore
    {
        private readonly IGrainFactory grainFactory;
        private readonly HashSet<string> users = new HashSet<string>();
        private bool isReserved;
        private AppCreated appCreated;

        public BackupApps(IStore<Guid> store, IGrainFactory grainFactory)
            : base(store)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public override Task RemoveAsync(Guid appId)
        {
            return RemoveSnapshotAsync<AppState>(appId);
        }

        public async override Task RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader)
        {
            switch (@event.Payload)
            {
                case AppCreated appCreated:
                    {
                        this.appCreated = appCreated;

                        var index = grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id);

                        if (!(isReserved = await index.ReserveAppAsync(appCreated.AppId.Id, appCreated.AppId.Name)))
                        {
                            throw new DomainException("The app id or name is not available.");
                        }

                        break;
                    }

                case AppContributorAssigned contributorAssigned:
                    users.Add(contributorAssigned.ContributorId);
                    break;

                case AppContributorRemoved contributorRemoved:
                    users.Remove(contributorRemoved.ContributorId);
                    break;
            }
        }

        public override async Task RestoreAsync(Guid appId, BackupReader reader)
        {
            await grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id).AddAppAsync(appCreated.AppId.Id, appCreated.AppId.Name);

            foreach (var user in users)
            {
                await grainFactory.GetGrain<IAppsByUserIndex>(user).AddAppAsync(appCreated.AppId.Id);
            }
        }

        public override async Task CleanupRestoreAsync(Guid appId, Exception exception)
        {
            if (isReserved)
            {
                var index = grainFactory.GetGrain<IAppsByNameIndex>(SingleGrain.Id);

                await index.ReserveAppAsync(appCreated.AppId.Id, appCreated.AppId.Name);
            }
        }
    }
}
