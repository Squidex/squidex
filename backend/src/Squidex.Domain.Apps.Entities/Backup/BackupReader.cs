// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Domain.Apps.Entities.Backup.Model;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupReader : DisposableObjectBase, IBackupReader
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

        public Task<Stream> OpenBlobAsync(string name,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var entry = GetEntry(name);

            return Task.FromResult(entry.Open());
        }

        public async Task<T> ReadJsonAsync<T>(string name,
            CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var entry = GetEntry(name);

            await using (var stream = entry.Open())
            {
                return serializer.Deserialize<T>(stream, null);
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

        public async IAsyncEnumerable<(string Stream, Envelope<IEvent> Event)> ReadEventsAsync(IStreamNameResolver streamNameResolver, IEventDataFormatter formatter,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(streamNameResolver, nameof(streamNameResolver));

            while (!ct.IsCancellationRequested)
            {
                var entry = archive.GetEntry(ArchiveHelper.GetEventPath(readEvents));

                if (entry == null)
                {
                    break;
                }

                await using (var stream = entry.Open())
                {
                    var storedEvent = serializer.Deserialize<CompatibleStoredEvent>(stream).ToStoredEvent();

                    var eventStream = storedEvent.StreamName;
                    var eventEnvelope = formatter.Parse(storedEvent);

                    yield return (eventStream, eventEnvelope);
                }

                readEvents++;
            }
        }
    }
}
