// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class BackupReaderWriterTests
    {
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();

        public BackupReaderWriterTests()
        {
            A.CallTo(() => streamNameResolver.WithNewId(A<string>.Ignored, A<Func<string, string>>.Ignored))
                .ReturnsLazily(new Func<string, Func<string, string>, string>((stream, idGenerator) => stream + "^2"));
        }

        [Fact]
        public async Task Should_write_and_read_events()
        {
            var stream = new MemoryStream();

            var sourceEvents = new List<StoredEvent>();

            using (var writer = new BackupWriter(stream, true))
            {
                for (var i = 0; i < 1000; i++)
                {
                    var eventData = new EventData { Type = i.ToString(), Metadata = i, Payload = i };
                    var eventStored = new StoredEvent("S", "1", 2, eventData);

                    if (i % 17 == 0)
                    {
                        var localI = i;

                        await writer.WriteBlobAsync(eventData.Type, innerStream =>
                        {
                            innerStream.WriteByte((byte)localI);

                            return TaskHelper.Done;
                        });
                    }
                    else if (i % 37 == 0)
                    {
                        await writer.WriteJsonAsync(eventData.Type, $"JSON_{i}");
                    }

                    writer.WriteEvent(eventStored);

                    sourceEvents.Add(eventStored);
                }
            }

            stream.Position = 0;

            var readEvents = new List<StoredEvent>();

            using (var reader = new BackupReader(stream))
            {
                await reader.ReadEventsAsync(streamNameResolver, async @event =>
                {
                    var i = int.Parse(@event.Data.Type);

                    if (i % 17 == 0)
                    {
                        await reader.ReadBlobAsync(@event.Data.Type, innerStream =>
                        {
                            var b = innerStream.ReadByte();

                            Assert.Equal((byte)i, b);

                            return TaskHelper.Done;
                        });
                    }
                    else if (i % 37 == 0)
                    {
                        var j = await reader.ReadJsonAttachmentAsync(@event.Data.Type);

                        Assert.Equal($"JSON_{i}", j.ToString());
                    }

                    readEvents.Add(@event);
                });
            }

            var sourceEventsWithNewStreamName =
                sourceEvents.Select(x =>
                    new StoredEvent(streamNameResolver.WithNewId(x.StreamName, null),
                        x.EventPosition,
                        x.EventStreamNumber,
                        x.Data)).ToList();

            readEvents.Should().BeEquivalentTo(sourceEventsWithNewStreamName);
        }
    }
}
