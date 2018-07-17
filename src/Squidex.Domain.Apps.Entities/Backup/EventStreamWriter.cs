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
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class EventStreamWriter : DisposableObjectBase
    {
        private const int MaxItemsPerFolder = 1000;
        private readonly ZipArchive archive;
        private int writtenEvents;
        private int writtenAttachments;

        public EventStreamWriter(Stream stream)
        {
            archive = new ZipArchive(stream, ZipArchiveMode.Update, true);
        }

        public async Task WriteEventAsync(EventData eventData, Func<Stream, Task> attachment = null)
        {
            var eventObject =
                new JObject(
                    new JProperty("type", eventData.Type),
                    new JProperty("payload", eventData.Payload),
                    new JProperty("metadata", eventData.Metadata));

            var eventFolder = writtenEvents / MaxItemsPerFolder;
            var eventPath = $"events/{eventFolder}/{writtenEvents}.json";
            var eventEntry = archive.GetEntry(eventPath) ?? archive.CreateEntry(eventPath);

            using (var stream = eventEntry.Open())
            {
                using (var textWriter = new StreamWriter(stream))
                {
                    using (var jsonWriter = new JsonTextWriter(textWriter))
                    {
                        await eventObject.WriteToAsync(jsonWriter);
                    }
                }
            }

            writtenEvents++;

            if (attachment != null)
            {
                var attachmentFolder = writtenAttachments / MaxItemsPerFolder;
                var attachmentPath = $"attachments/{attachmentFolder}/{writtenEvents}.blob";
                var attachmentEntry = archive.GetEntry(attachmentPath) ?? archive.CreateEntry(attachmentPath);

                using (var stream = attachmentEntry.Open())
                {
                    await attachment(stream);
                }

                writtenAttachments++;
            }
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                archive.Dispose();
            }
        }
    }
}
