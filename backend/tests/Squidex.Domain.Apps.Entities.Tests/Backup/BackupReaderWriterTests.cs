// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class BackupReaderWriterTests
    {
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly IJsonSerializer serializer = TestUtils.DefaultSerializer;
        private readonly IEventDataFormatter formatter;
        private readonly TypeNameRegistry typeNameRegistry = new TypeNameRegistry();

        [TypeName(nameof(MyEvent))]
        public sealed class MyEvent : IEvent
        {
            public Guid Id { get; set; } = Guid.NewGuid();
        }

        public BackupReaderWriterTests()
        {
            typeNameRegistry.Map(typeof(MyEvent));

            formatter = new DefaultEventDataFormatter(typeNameRegistry, serializer);
        }

        [Fact]
        public async Task Should_not_write_blob_if_handler_failed()
        {
            var file = "File.json";

            await TestReaderWriterAsync(BackupVersion.V1, async writer =>
            {
                try
                {
                    await using (var stream = await writer.OpenBlobAsync(file))
                    {
                        throw new InvalidOperationException();
                    }
                }
                catch
                {
                    return;
                }
            }, async reader =>
            {
                await Assert.ThrowsAsync<FileNotFoundException>(() => ReadGuidAsync(reader, file));
            });
        }

        [Fact]
        public async Task Should_read_and_write_json_async()
        {
            var file = "File.json";

            var value = Guid.NewGuid();

            await TestReaderWriterAsync(BackupVersion.V1, async writer =>
            {
                await WriteJsonGuidAsync(writer, file, value);
            }, async reader =>
            {
                var read = await ReadJsonGuidAsync(reader, file);

                Assert.Equal(value, read);
            });
        }

        [Fact]
        public async Task Should_read_and_write_blob_async()
        {
            var file = "File.json";

            var value = Guid.NewGuid();

            await TestReaderWriterAsync(BackupVersion.V1, async writer =>
            {
                await WriteGuidAsync(writer, file, value);
            }, async reader =>
            {
                var read = await ReadGuidAsync(reader, file);

                Assert.Equal(value, read);
            });
        }

        [Fact]
        public async Task Should_throw_exception_if_json_not_found()
        {
            await TestReaderWriterAsync(BackupVersion.V1, writer =>
            {
                return Task.CompletedTask;
            }, async reader =>
            {
                await Assert.ThrowsAsync<FileNotFoundException>(() => reader.ReadJsonAsync<int>("404"));
            });
        }

        [Fact]
        public async Task Should_throw_exception_if_blob_not_found()
        {
            await TestReaderWriterAsync(BackupVersion.V1, writer =>
            {
                return Task.CompletedTask;
            }, async reader =>
            {
                await Assert.ThrowsAsync<FileNotFoundException>(() => reader.OpenBlobAsync("404"));
            });
        }

        [Theory]
        [InlineData(BackupVersion.V1)]
        [InlineData(BackupVersion.V2)]
        public async Task Should_write_and_read_events_to_backup(BackupVersion version)
        {
            var randomGenerator = new Random();
            var randomDomainIds = new List<DomainId>();

            for (var i = 0; i < 100; i++)
            {
                randomDomainIds.Add(DomainId.NewGuid());
            }

            DomainId RandomDomainId()
            {
                return randomDomainIds[randomGenerator.Next(randomDomainIds.Count)];
            }

            var sourceEvents = new List<(string Stream, Envelope<MyEvent> Event)>();

            for (var i = 0; i < 200; i++)
            {
                var @event = new MyEvent();

                var envelope = Envelope.Create(@event);

                envelope.Headers.Add("Id", @event.Id.ToString());
                envelope.Headers.Add("Index", i);

                sourceEvents.Add(($"My-{RandomDomainId()}", envelope));
            }

            await TestReaderWriterAsync(version, async writer =>
            {
                foreach (var (stream, envelope) in sourceEvents)
                {
                    var eventData = formatter.ToEventData(envelope, Guid.NewGuid(), true);
                    var eventStored = new StoredEvent(stream, "1", 2, eventData);

                    var index = int.Parse(envelope.Headers["Index"].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);

                    if (index % 17 == 0)
                    {
                        await WriteGuidAsync(writer, index.ToString(CultureInfo.InvariantCulture), envelope.Payload.Id);
                    }
                    else if (index % 37 == 0)
                    {
                        await WriteJsonGuidAsync(writer, index.ToString(CultureInfo.InvariantCulture), envelope.Payload.Id);
                    }

                    writer.WriteEvent(eventStored);
                }
            }, async reader =>
            {
                var targetEvents = new List<(string Stream, Envelope<IEvent> Event)>();

                await foreach (var @event in reader.ReadEventsAsync(streamNameResolver, formatter))
                {
                    var index = int.Parse(@event.Event.Headers["Index"].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);

                    var id = Guid.Parse(@event.Event.Headers["Id"].ToString());

                    if (index % 17 == 0)
                    {
                        var guid = await ReadGuidAsync(reader, index.ToString(CultureInfo.InvariantCulture));

                        Assert.Equal(id, guid);
                    }
                    else if (index % 37 == 0)
                    {
                        var guid = await ReadJsonGuidAsync(reader, index.ToString(CultureInfo.InvariantCulture));

                        Assert.Equal(id, guid);
                    }

                    targetEvents.Add(@event);
                }

                for (var i = 0; i < targetEvents.Count; i++)
                {
                    var targetEvent = targetEvents[i].Event.To<MyEvent>();
                    var targetStream = targetEvents[i].Stream;

                    var sourceEvent = sourceEvents[i].Event.To<MyEvent>();
                    var sourceStream = sourceEvents[i].Stream;

                    Assert.Equal(sourceEvent.Payload.Id, targetEvent.Payload.Id);
                    Assert.Equal(sourceStream, targetStream);
                }
            });
        }

        private static Task<Guid> ReadJsonGuidAsync(IBackupReader reader, string file)
        {
            return reader.ReadJsonAsync<Guid>(file);
        }

        private static Task WriteJsonGuidAsync(IBackupWriter writer, string file, Guid value)
        {
            return writer.WriteJsonAsync(file, value);
        }

        private static async Task WriteGuidAsync(IBackupWriter writer, string file, Guid value)
        {
            await using (var stream = await writer.OpenBlobAsync(file))
            {
                await stream.WriteAsync(value.ToByteArray());
            }
        }

        private static async Task<Guid> ReadGuidAsync(IBackupReader reader, string file)
        {
            var read = Guid.Empty;

            await using (var stream = await reader.OpenBlobAsync(file))
            {
                var buffer = new byte[16];

                _ = await stream.ReadAsync(buffer);

                read = new Guid(buffer);
            }

            return read;
        }

        private async Task TestReaderWriterAsync(BackupVersion version, Func<IBackupWriter, Task> write, Func<IBackupReader, Task> read)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BackupWriter(serializer, stream, true, version))
                {
                    await write(writer);
                }

                stream.Position = 0;

                using (var reader = new BackupReader(serializer, stream))
                {
                    await read(reader);
                }
            }
        }
    }
}
