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
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupReader : DisposableObjectBase
    {
        private readonly GuidMapper guidMapper = new GuidMapper();
        private readonly ZipArchive archive;
        private readonly IJsonSerializer serializer;
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

        public Guid OldGuid(Guid newId)
        {
            return guidMapper.OldGuid(newId);
        }

        public Task<T> ReadJsonAttachmentAsync<T>(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var attachmentEntry = archive.GetEntry(ArchiveHelper.GetAttachmentPath(name));

            if (attachmentEntry == null)
            {
                throw new FileNotFoundException("Cannot find attachment.", name);
            }

            T result;

            using (var stream = attachmentEntry.Open())
            {
                result = serializer.Deserialize<T>(stream, null, guidMapper.NewGuidOrValue);
            }

            readAttachments++;

            return Task.FromResult(result);
        }

        public async Task ReadBlobAsync(string name, Func<Stream, Task> handler)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(handler, nameof(handler));

            var attachmentEntry = archive.GetEntry(ArchiveHelper.GetAttachmentPath(name));

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

        public async Task ReadEventsAsync(IStreamNameResolver streamNameResolver, IEventDataFormatter formatter, Func<(string Stream, Envelope<IEvent> Event), Task> handler)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(formatter, nameof(formatter));
            Guard.NotNull(streamNameResolver, nameof(streamNameResolver));

            while (true)
            {
                var eventEntry = archive.GetEntry(ArchiveHelper.GetEventPath(readEvents));

                if (eventEntry == null)
                {
                    break;
                }

                using (var stream = eventEntry.Open())
                {
                    var storedEvent = serializer.Deserialize<StoredEvent>(stream);

                    var eventStream = streamNameResolver.WithNewId(storedEvent.StreamName, guidMapper.NewGuidOrNull);
                    var eventEnvelope = formatter.Parse(storedEvent.Data, true, guidMapper.NewGuidOrValue);

                    await handler((eventStream, eventEnvelope));
                }

                readEvents++;
            }
        }
    }
}
