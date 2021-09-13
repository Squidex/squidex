// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupService : IBackupService, IDeleter
    {
        private readonly IGrainFactory grainFactory;

        public BackupService(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return BackupGrain(app.Id).ClearAsync();
        }

        public Task StartBackupAsync(DomainId appId, RefToken actor)
        {
            return BackupGrain(appId).BackupAsync(actor);
        }

        public Task StartRestoreAsync(RefToken actor, Uri url, string? newAppName)
        {
            return RestoreGrain().RestoreAsync(url, actor, newAppName);
        }

        public Task DeleteBackupAsync(DomainId appId, DomainId backupId,
            CancellationToken ct = default)
        {
            return BackupGrain(appId).DeleteAsync(backupId);
        }

        public async Task<IRestoreJob?> GetRestoreAsync(
            CancellationToken ct = default)
        {
            var state = await RestoreGrain().GetStateAsync();

            return state.Value;
        }

        public async Task<List<IBackupJob>> GetBackupsAsync(DomainId appId,
            CancellationToken ct = default)
        {
            var state = await BackupGrain(appId).GetStateAsync();

            return state.Value;
        }

        public async Task<IBackupJob?> GetBackupAsync(DomainId appId, DomainId backupId,
            CancellationToken ct = default)
        {
            var state = await BackupGrain(appId).GetStateAsync();

            return state.Value.Find(x => x.Id == backupId);
        }

        private IRestoreGrain RestoreGrain()
        {
            return grainFactory.GetGrain<IRestoreGrain>(SingleGrain.Id);
        }

        private IBackupGrain BackupGrain(DomainId appId)
        {
            return grainFactory.GetGrain<IBackupGrain>(appId.ToString());
        }
    }
}
