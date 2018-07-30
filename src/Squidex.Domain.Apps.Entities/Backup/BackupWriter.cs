// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Entities.Backup.Archive;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupWriter : DisposableObjectBase
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();
        private readonly ZipArchive archive;
        private int writtenEvents;
        private int writtenAttachments;

        public int WrittenEvents
        {
            get { return writtenEvents; }
        }

        public int WrittenAttachments
        {
            get { return writtenAttachments; }
        }

        public BackupWriter(Stream stream)
        {
            archive = new ZipArchive(stream, ZipArchiveMode.Update, true);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                archive.Dispose();
            }
        }

        public async Task WriteAttachmentAsync(string name, Func<Stream, Task> handler)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(handler, nameof(handler));

            var attachmentEntry = archive.CreateEntry(ArchiveHelper.GetAttachmentPath(name));

            using (var stream = attachmentEntry.Open())
            {
                await handler(stream);
            }

            writtenAttachments++;
        }

        public void WriteEvent(StoredEvent storedEvent)
        {
            var eventEntry = archive.CreateEntry(ArchiveHelper.GetEventPath(writtenEvents));

            using (var stream = eventEntry.Open())
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    JsonSerializer.Serialize(textWriter, storedEvent);
                }
            }

            writtenEvents++;
        }
    }
}
