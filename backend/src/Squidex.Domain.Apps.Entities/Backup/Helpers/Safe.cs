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
        public static async Task DeleteAsync(IAssetStore assetStore, Guid id, ISemanticLog log)
        {
            try
            {
                await assetStore.DeleteAsync(id, 0, null);
            }
            catch (Exception ex)
            {
                log.LogError(ex, id.ToString(), (logOperationId, w) => w
                    .WriteProperty("action", "deleteBackup")
                    .WriteProperty("status", "failed")
                    .WriteProperty("operationId", logOperationId));
            }
        }

        public static async Task CleanupRestoreErrorAsync(IBackupHandler handler, Guid appId, Guid id, ISemanticLog log)
        {
            try
            {
                await handler.CleanupRestoreErrorAsync(appId);
            }
            catch (Exception ex)
            {
                log.LogError(ex, id.ToString(), (logOperationId, w) => w
                    .WriteProperty("action", "cleanupRestore")
                    .WriteProperty("status", "failed")
                    .WriteProperty("operationId", logOperationId));
            }
        }
    }
}
