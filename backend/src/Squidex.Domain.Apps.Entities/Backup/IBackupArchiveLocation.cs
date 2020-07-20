// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupArchiveLocation
    {
        Stream OpenStream(DomainId backupId);

        Task<IBackupWriter> OpenWriterAsync(Stream stream);

        Task<IBackupReader> OpenReaderAsync(Uri url, DomainId id);
    }
}
