// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
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

        public Task<Stream> OpenBlobAsync(string name,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            writtenAttachments++;

            var entry = GetEntry(name);

            return Task.FromResult(entry.Open());
        }

        public async Task WriteJsonAsync(string name, object value,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            writtenAttachments++;

            var entry = GetEntry(name);

            await using (var stream = entry.Open())
            {
                serializer.Serialize(value, stream);
            }
        }

        private ZipArchiveEntry GetEntry(string name)
        {
            return archive.CreateEntry(ArchiveHelper.GetAttachmentPath(name));
        }

        public void WriteEvent(StoredEvent storedEvent,
            CancellationToken ct = default)
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
