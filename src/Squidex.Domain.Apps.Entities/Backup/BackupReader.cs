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
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupReader : DisposableObjectBase
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();
        private readonly GuidMapper guidMapper = new GuidMapper();
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

        public async Task<JToken> ReadJsonAttachmentAsync(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            var attachmentEntry = archive.GetEntry(ArchiveHelper.GetAttachmentPath(name));

            if (attachmentEntry == null)
            {
                throw new FileNotFoundException("Cannot find attachment.", name);
            }

            JToken result;

            using (var stream = attachmentEntry.Open())
            {
                using (var textReader = new StreamReader(stream))
                {
                    using (var jsonReader = new JsonTextReader(textReader))
                    {
                        result = await JToken.ReadFromAsync(jsonReader);

                        guidMapper.NewGuids(result);
                    }
                }
            }

            readAttachments++;

            return result;
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

        public async Task ReadEventsAsync(IStreamNameResolver streamNameResolver, Func<StoredEvent, Task> handler)
        {
            Guard.NotNull(handler, nameof(handler));
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
                    using (var textReader = new StreamReader(stream))
                    {
                        using (var jsonReader = new JsonTextReader(textReader))
                        {
                            var storedEvent = Serializer.Deserialize<StoredEvent>(jsonReader);

                            storedEvent.Data.Payload = guidMapper.NewGuids(storedEvent.Data.Payload);
                            storedEvent.Data.Metadata = guidMapper.NewGuids(storedEvent.Data.Metadata);

                            var streamName = streamNameResolver.WithNewId(storedEvent.StreamName, guidMapper.NewGuidString);

                            storedEvent = new StoredEvent(streamName,
                                storedEvent.EventPosition,
                                storedEvent.EventStreamNumber,
                                storedEvent.Data);

                            await handler(storedEvent);
                        }
                    }
                }

                readEvents++;
            }
        }
    }
}
