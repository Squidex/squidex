// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class TempFolderBackupArchiveLocation : IBackupArchiveLocation
    {
        public Task<Stream> OpenStreamAsync(Guid backupId)
        {
            var tempFile = GetTempFile(backupId);

            return Task.FromResult<Stream>(new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite));
        }

        public Task DeleteArchiveAsync(Guid backupId)
        {
            var tempFile = GetTempFile(backupId);

            try
            {
                File.Delete(tempFile);
            }
            catch (IOException)
            {
            }

            return TaskHelper.Done;
        }

        private static string GetTempFile(Guid backupId)
        {
            return Path.Combine(Path.GetTempPath(), backupId + ".zip");
        }
    }
}
