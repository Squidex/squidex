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
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class BackupReader : DisposableObjectBase, IBackupReader
    {
        private readonly ZipArchive archive;
        private readonly IJsonSerializer serializer;
        private int readEvents;
        private int readAttachments;

        public int ReadEvents
        {
            get => readEvents;
        }

        public int ReadAttachments
        {
            get => readAttachments;
        }

        public BackupReader(IJsonSerializer serializer, Stream stream)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;

            archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                archive.Dispose();
            }
        }

        public Task<T> ReadJsonAsync<T>(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var entry = GetEntry(name);

            using (var stream = entry.Open())
            {
                return Task.FromResult(serializer.Deserialize<T>(stream, null));
            }
        }

        public async Task ReadBlobAsync(string name, Func<Stream, Task> handler)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(handler, nameof(handler));

            var entry = GetEntry(name);

            using (var stream = entry.Open())
            {
                await handler(stream);
            }
        }

        private ZipArchiveEntry GetEntry(string name)
        {
            var attachmentEntry = archive.GetEntry(ArchiveHelper.GetAttachmentPath(name));

            if (attachmentEntry == null || attachmentEntry.Length == 0)
            {
                throw new FileNotFoundException("Cannot find attachment.", name);
            }

            readAttachments++;

            return attachmentEntry;
        }

        public async Task ReadEventsAsync(IStreamNameResolver streamNameResolver, IEventDataFormatter formatter, Func<(string Stream, Envelope<IEvent> Event), Task> handler)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(streamNameResolver, nameof(streamNameResolver));

            while (true)
            {
                var entry = archive.GetEntry(ArchiveHelper.GetEventPath(readEvents));

                if (entry == null)
                {
                    break;
                }

                using (var stream = entry.Open())
                {
                    var storedEvent = serializer.Deserialize<CompatibleStoredEvent>(stream).ToStoredEvent();

                    var eventStream = storedEvent.StreamName;
                    var eventEnvelope = formatter.Parse(storedEvent);

                    await handler((eventStream, eventEnvelope));
                }

                readEvents++;
            }
        }
    }
}
