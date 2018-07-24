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
using Xunit;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public class EventStreamTests
    {
        public sealed class EventInfo
        {
            public EventData Data { get; set; }

            public byte[] Attachment { get; set; }
        }

        [Fact]
        public async Task Should_write_and_read_events()
        {
            var stream = new MemoryStream();

            var sourceEvents = new List<EventInfo>();

            for (var i = 0; i < 1000; i++)
            {
                var eventData = new EventData { Type = i.ToString(), Metadata = i, Payload = i };
                var eventInfo = new EventInfo { Data = eventData };

                if (i % 10 == 0)
                {
                    eventInfo.Attachment = new byte[] { (byte)i };
                }

                sourceEvents.Add(eventInfo);
            }

            using (var reader = new EventStreamWriter(stream))
            {
                foreach (var @event in sourceEvents)
                {
                    if (@event.Attachment == null)
                    {
                        await reader.WriteEventAsync(@event.Data);
                    }
                    else
                    {
                        await reader.WriteEventAsync(@event.Data, s => s.WriteAsync(@event.Attachment, 0, 1));
                    }
                }
            }

            stream.Position = 0;

            var readEvents = new List<EventInfo>();

            using (var reader = new EventStreamReader(stream))
            {
                await reader.ReadEventsAsync(async (eventData, attachment) =>
                {
                    var eventInfo = new EventInfo { Data = eventData };

                    if (attachment != null)
                    {
                        eventInfo.Attachment = new byte[1];

                        await attachment.ReadAsync(eventInfo.Attachment, 0, 1);
                    }

                    readEvents.Add(eventInfo);
                });
            }

            readEvents.Should().BeEquivalentTo(sourceEvents);
        }
    }
}
