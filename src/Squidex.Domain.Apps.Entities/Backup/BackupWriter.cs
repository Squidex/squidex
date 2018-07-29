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
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupWriter : DisposableObjectBase
    {
        private const int MaxEventsPerFolder = 1000;
        private const int MaxAttachmentFolders = 1000;
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

            var attachmentFolder = Math.Abs(name.GetHashCode() % MaxAttachmentFolders);
            var attachmentPath = $"attachments/{attachmentFolder}/{name}";
            var attachmentEntry = archive.CreateEntry(attachmentPath);

            using (var stream = attachmentEntry.Open())
            {
                await handler(stream);
            }

            writtenAttachments++;
        }

        public void WriteEvent(StoredEvent storedEvent)
        {
            var eventFolder = writtenEvents / MaxEventsPerFolder;
            var eventPath = $"events/{eventFolder}/{writtenEvents}.json";
            var eventEntry = archive.GetEntry(eventPath) ?? archive.CreateEntry(eventPath);

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
