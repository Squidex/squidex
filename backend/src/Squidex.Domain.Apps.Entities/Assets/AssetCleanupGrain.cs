// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Orleans.Runtime;
using tusdotnet.Interfaces;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCleanupGrain : Grain, IRemindable, IAssetCleanupGrain
    {
        private readonly ITusExpirationStore expirationStore;

        public AssetCleanupGrain(ITusExpirationStore expirationStore)
        {
            this.expirationStore = expirationStore;
        }

        public override Task OnActivateAsync()
        {
            RegisterOrUpdateReminder("Cleanup", TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return base.OnActivateAsync();
        }

        public Task ActivateAsync()
        {
            return Task.CompletedTask;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return expirationStore.RemoveExpiredFilesAsync(default);
        }
    }
}
