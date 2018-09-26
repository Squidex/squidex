// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Backup.Helpers
{
    public static class Safe
    {
        public static async Task DeleteAsync(IBackupArchiveLocation backupArchiveLocation, Guid id, ISemanticLog log)
        {
            try
            {
                await backupArchiveLocation.DeleteArchiveAsync(id);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "deleteArchive")
                    .WriteProperty("status", "failed")
                    .WriteProperty("operationId", id.ToString()));
            }
        }

        public static async Task DeleteAsync(IAssetStore assetStore, Guid id, ISemanticLog log)
        {
            try
            {
                await assetStore.DeleteAsync(id.ToString(), 0, null);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "deleteBackup")
                    .WriteProperty("status", "failed")
                    .WriteProperty("operationId", id.ToString()));
            }
        }

        public static async Task CleanupRestoreAsync(BackupHandler handler, Guid appId, Guid id, ISemanticLog log)
        {
            try
            {
                await handler.CleanupRestoreAsync(appId);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "cleanupRestore")
                    .WriteProperty("status", "failed")
                    .WriteProperty("operationId", id.ToString()));
            }
        }
    }
}
