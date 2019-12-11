// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupArchiveStore
    {
        Task UploadAsync(Guid backupId, Stream stream, CancellationToken ct = default);

        Task DownloadAsync(Guid backupId, Stream stream, CancellationToken ct = default);

        Task DeleteAsync(Guid backupId);
    }
}
