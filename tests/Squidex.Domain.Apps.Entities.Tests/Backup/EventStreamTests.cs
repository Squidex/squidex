// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class EventStreamTests
    {
        [Fact]
        public async Task Should_write_and_read_events()
        {
            var stream = new MemoryStream();

            var sourceEvents = new List<StoredEvent>();

            using (var writer = new BackupWriter(stream))
            {
                for (var i = 0; i < 1000; i++)
                {
                    var eventData = new EventData { Type = i.ToString(), Metadata = i, Payload = i };
                    var eventStored = new StoredEvent("S", "1", 2, eventData);

                    if (i % 10 == 0)
                    {
                        await writer.WriteAttachmentAsync(eventData.Type, innerStream =>
                        {
                            return innerStream.WriteAsync(new byte[] { (byte)i }, 0, 1);
                        });
                    }

                    writer.WriteEvent(eventStored);

                    sourceEvents.Add(eventStored);
                }
            }

            stream.Position = 0;

            var readEvents = new List<StoredEvent>();

            using (var reader = new BackupReader(stream))
            {
                await reader.ReadEventsAsync(async @event =>
                {
                    var i = int.Parse(@event.Data.Type);

                    if (i % 10 == 0)
                    {
                        await reader.ReadAttachmentAsync(@event.Data.Type, innerStream =>
                        {
                            var b = innerStream.ReadByte();

                            Assert.Equal((byte)i, b);

                            return TaskHelper.Done;
                        });
                    }

                    readEvents.Add(@event);
                });
            }

            readEvents.Should().BeEquivalentTo(sourceEvents);
        }
    }
}
