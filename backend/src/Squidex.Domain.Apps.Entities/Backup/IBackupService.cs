// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupService
    {
        Task StartBackupAsync(Guid appId, RefToken actor);

        Task StartRestoreAsync(RefToken actor, Uri url, string? newAppName);

        Task<IRestoreJob?> GetRestoreAsync();

        Task<List<IBackupJob>> GetBackupsAsync(Guid appId);

        Task<IBackupJob?> GetBackupAsync(Guid appId, Guid backupId);

        Task DeleteBackupAsync(Guid appId, Guid backupId);
    }
}
