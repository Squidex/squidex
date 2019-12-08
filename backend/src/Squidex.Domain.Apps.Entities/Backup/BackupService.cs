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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupService : IBackupService
    {
        private readonly IGrainFactory grainFactory;

        public BackupService(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory);

            this.grainFactory = grainFactory;
        }

        public Task StartBackupAsync(Guid appId, RefToken actor)
        {
            var grain = grainFactory.GetGrain<IBackupGrain>(appId);

            return grain.BackupAsync(actor);
        }

        public Task StartRestoreAsync(RefToken actor, Uri url, string? newAppName)
        {
            var grain = grainFactory.GetGrain<IRestoreGrain>(SingleGrain.Id);

            return grain.RestoreAsync(url, actor, newAppName);
        }

        public async Task<IRestoreJob?> GetRestoreAsync()
        {
            var grain = grainFactory.GetGrain<IRestoreGrain>(SingleGrain.Id);

            var state = await grain.GetStateAsync();

            return state.Value;
        }

        public async Task<List<IBackupJob>> GetBackupsAsync(Guid appId)
        {
            var grain = grainFactory.GetGrain<IBackupGrain>(appId);

            var state = await grain.GetStateAsync();

            return state.Value;
        }

        public async Task<IBackupJob?> GetBackupAsync(Guid appId, Guid backupId)
        {
            var grain = grainFactory.GetGrain<IBackupGrain>(appId);

            var state = await grain.GetStateAsync();

            return state.Value.Find(x => x.Id == backupId);
        }

        public Task DeleteBackupAsync(Guid appId, Guid backupId)
        {
            var grain = grainFactory.GetGrain<IBackupGrain>(appId);

            return grain.DeleteAsync(backupId);
        }
    }
}
