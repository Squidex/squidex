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
    public sealed class BackupReader : DisposableObjectBase
    {
        private const int MaxEventsPerFolder = 1000;
        private const int MaxAttachmentFolders = 1000;
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();
        private readonly ZipArchive archive;
        private int readEvents;
        private int readAttachments;

        public int ReadEvents
        {
            get { return readEvents; }
        }

        public int ReadAttachments
        {
            get { return readAttachments; }
        }

        public BackupReader(Stream stream)
        {
            archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                archive.Dispose();
            }
        }

        public async Task ReadAttachmentAsync(string name, Func<Stream, Task> handler)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(handler, nameof(handler));

            var attachmentFolder = Math.Abs(name.GetHashCode() % MaxAttachmentFolders);
            var attachmentPath = $"attachments/{attachmentFolder}/{name}";
            var attachmentEntry = archive.GetEntry(attachmentPath);

            if (attachmentEntry == null)
            {
                throw new FileNotFoundException("Cannot find attachment.", name);
            }

            using (var stream = attachmentEntry.Open())
            {
                await handler(stream);
            }

            readAttachments++;
        }

        public async Task ReadEventsAsync(Func<StoredEvent, Task> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            while (true)
            {
                var eventFolder = readEvents / MaxEventsPerFolder;
                var eventPath = $"events/{eventFolder}/{readEvents}.json";
                var eventEntry = archive.GetEntry(eventPath);

                if (eventEntry == null)
                {
                    break;
                }

                using (var stream = eventEntry.Open())
                {
                    using (var textReader = new StreamReader(stream))
                    {
                        var storedEvent = (StoredEvent)JsonSerializer.Deserialize(textReader, typeof(StoredEvent));

                        await handler(storedEvent);
                    }
                }

                readEvents++;
            }
        }
    }
}
