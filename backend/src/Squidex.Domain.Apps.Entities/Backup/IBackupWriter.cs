// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public interface IBackupWriter : IDisposable
    {
        int WrittenAttachments { get; }

        int WrittenEvents { get; }

        Task WriteBlobAsync(string name, Func<Stream, Task> handler);

        void WriteEvent(StoredEvent storedEvent);

        Task WriteJsonAsync(string name, object value);
    }
}