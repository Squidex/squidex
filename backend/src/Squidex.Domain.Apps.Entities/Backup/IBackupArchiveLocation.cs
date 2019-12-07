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
        Task<Stream> OpenStreamAsync(string backupId);

        Task<IBackupWriter> OpenWriterAsync(Stream stream);

        Task<IBackupReader> OpenReaderAsync(Uri url, string id);

        Task DeleteArchiveAsync(string backupId);
    }
}
