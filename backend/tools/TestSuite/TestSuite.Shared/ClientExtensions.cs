// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;

namespace TestSuite
{
    public static class ClientExtensions
    {
        public static async Task<BackupJobDto> WaitForBackupAsync(this IBackupsClient backupsClient, string app, TimeSpan timeout)
        {
            try
            {
                using var cts = new CancellationTokenSource(timeout);

                while (!cts.IsCancellationRequested)
                {
                    var backups = await backupsClient.GetBackupsAsync(app, cts.Token);
                    var backup = backups.Items.Find(x => x.Status == JobStatus.Completed || x.Status == JobStatus.Failed);

                    if (backup != null)
                    {
                        return backup;
                    }

                    await Task.Delay(200, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }

            return null;
        }

        public static async Task<RestoreJobDto> WaitForRestoreAsync(this IBackupsClient backupsClient, Uri url, TimeSpan timeout)
        {
            try
            {
                using var cts = new CancellationTokenSource(timeout);

                while (!cts.IsCancellationRequested)
                {
                    var restore = await backupsClient.GetRestoreJobAsync(cts.Token);

                    if (restore.Url == url && restore.Status is JobStatus.Completed or JobStatus.Failed)
                    {
                        return restore;
                    }

                    await Task.Delay(200, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }

            return null;
        }
    }
}
