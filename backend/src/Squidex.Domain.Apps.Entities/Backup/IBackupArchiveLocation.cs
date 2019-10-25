// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupArchiveLocation
    {
        Task<Stream> OpenStreamAsync(string backupId);

        Task DeleteArchiveAsync(string backupId);
    }
}
