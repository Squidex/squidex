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
    public sealed class EventStreamReader : DisposableObjectBase
    {
        private const int MaxItemsPerFolder = 1000;
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();
        private readonly ZipArchive archive;

        public EventStreamReader(Stream stream)
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

        public async Task ReadEventsAsync(Func<StoredEvent, Stream, Task> eventHandler)
        {
            Guard.NotNull(eventHandler, nameof(eventHandler));

            var readEvents = 0;
            var readAttachments = 0;

            while (true)
            {
                var eventFolder = readEvents / MaxItemsPerFolder;
                var eventPath = $"events/{eventFolder}/{readEvents}.json";
                var eventEntry = archive.GetEntry(eventPath);

                if (eventEntry == null)
                {
                    break;
                }

                StoredEvent eventData;

                using (var stream = eventEntry.Open())
                {
                    using (var textReader = new StreamReader(stream))
                    {
                        eventData = (StoredEvent)JsonSerializer.Deserialize(textReader, typeof(StoredEvent));
                    }
                }

                var attachmentFolder = readAttachments / MaxItemsPerFolder;
                var attachmentPath = $"attachments/{attachmentFolder}/{readEvents}.blob";
                var attachmentEntry = archive.GetEntry(attachmentPath);

                if (attachmentEntry != null)
                {
                    using (var stream = attachmentEntry.Open())
                    {
                        await eventHandler(eventData, stream);

                        readAttachments++;
                    }
                }
                else
                {
                    await eventHandler(eventData, null);
                }

                readEvents++;
            }
        }
    }
}
