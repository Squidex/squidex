// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupArchiveStore
    {
        Task UploadAsync(DomainId backupId, Stream stream,
            CancellationToken ct = default);

        Task DownloadAsync(DomainId backupId, Stream stream,
            CancellationToken ct = default);

        Task DeleteAsync(DomainId backupId,
            CancellationToken ct = default);
    }
}
