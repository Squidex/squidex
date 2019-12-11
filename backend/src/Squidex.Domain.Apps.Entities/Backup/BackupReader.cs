﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Domain.Apps.Entities.Backup.Model;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.States;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class BackupReader : DisposableObjectBase, IBackupReader
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
            Guard.NotNull(serializer);

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
            Guard.NotNullOrEmpty(name);

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
            Guard.NotNullOrEmpty(name);
            Guard.NotNull(handler);

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
            Guard.NotNull(handler);
            Guard.NotNull(formatter);
            Guard.NotNull(streamNameResolver);

            while (true)
            {
                var eventEntry = archive.GetEntry(ArchiveHelper.GetEventPath(readEvents));

                if (eventEntry == null)
                {
                    break;
                }

                using (var stream = eventEntry.Open())
                {
                    var (streamName, data) = serializer.Deserialize<CompatibleStoredEvent>(stream).ToEvent();

                    MapHeaders(data);

                    var eventStream = streamNameResolver.WithNewId(streamName, guidMapper.NewGuidOrNull);
                    var eventEnvelope = formatter.Parse(data, guidMapper.NewGuidOrValue);

                    await handler((eventStream, eventEnvelope));
                }

                readEvents++;
            }
        }

        private void MapHeaders(EventData data)
        {
            foreach (var (key, value) in data.Headers.ToList())
            {
                if (value.Type == JsonValueType.String)
                {
                    var newGuid = guidMapper.NewGuidOrNull(value.ToString());

                    if (newGuid != null)
                    {
                        data.Headers.Add(key, newGuid);
                    }
                }
            }
        }
    }
}
