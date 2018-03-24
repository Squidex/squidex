// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupArchiveLocation
    {
        Task<Stream> OpenStreamAsync(Guid backupId);

        Task DeleteArchiveAsync(Guid backupId);
    }
}
