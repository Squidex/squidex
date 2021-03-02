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
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Domain.Apps.Entities.Backup.Model;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupWriter : DisposableObjectBase, IBackupWriter
    {
        private readonly ZipArchive archive;
        private readonly IJsonSerializer serializer;
        private readonly Func<StoredEvent, CompatibleStoredEvent> converter;
        private int writtenEvents;
        private int writtenAttachments;

        public int WrittenEvents
        {
            get => writtenEvents;
        }

        public int WrittenAttachments
        {
            get => writtenAttachments;
        }

        public BackupWriter(IJsonSerializer serializer, Stream stream, bool keepOpen = false, BackupVersion version = BackupVersion.V2)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;

            converter =
                version == BackupVersion.V1 ?
                    new Func<StoredEvent, CompatibleStoredEvent>(CompatibleStoredEvent.V1) :
                    new Func<StoredEvent, CompatibleStoredEvent>(CompatibleStoredEvent.V2);

            archive = new ZipArchive(stream, ZipArchiveMode.Create, keepOpen);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                archive.Dispose();
            }
        }

        public Task WriteJsonAsync(string name, object value)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var attachmentEntry = archive.CreateEntry(ArchiveHelper.GetAttachmentPath(name));

            using (var stream = attachmentEntry.Open())
            {
                serializer.Serialize(value, stream);
            }

            writtenAttachments++;

            return Task.CompletedTask;
        }

        public async Task WriteBlobAsync(string name, Func<Stream, Task> handler)
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
            Guard.NotNull(storedEvent, nameof(storedEvent));

            var eventEntry = archive.CreateEntry(ArchiveHelper.GetEventPath(writtenEvents));

            using (var stream = eventEntry.Open())
            {
                var @event = converter(storedEvent);

                serializer.Serialize(@event, stream);
            }

            writtenEvents++;
        }
    }
}
