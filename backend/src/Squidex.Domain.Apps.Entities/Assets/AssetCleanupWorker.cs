// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;
using Squidex.Infrastructure.Timers;
using tusdotnet.Interfaces;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCleanupWorker : IInitializable
    {
        private readonly ITusExpirationStore expirationStore;
        private readonly CompletionTimer timer;

        public AssetCleanupWorker(ITusExpirationStore expirationStore)
        {
            this.expirationStore = expirationStore;

            timer = new CompletionTimer((int)TimeSpan.FromMinutes(10).TotalMilliseconds, CleanupAsync);
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task ReleaseAsync(
            CancellationToken ct)
        {
            return timer.StopAsync();
        }

        public Task CleanupAsync(
            CancellationToken ct)
        {
            return expirationStore.RemoveExpiredFilesAsync(ct);
        }
    }
}
