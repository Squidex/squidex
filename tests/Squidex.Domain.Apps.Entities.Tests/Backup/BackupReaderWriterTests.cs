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
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
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
            public Guid GuidRaw { get; set; }

            public NamedId<Guid> GuidNamed { get; set; }

            public Dictionary<Guid, string> Values { get; set; }
        }

        public BackupReaderWriterTests()
        {
            typeNameRegistry.Map(typeof(MyEvent));

            formatter = new DefaultEventDataFormatter(typeNameRegistry, serializer);

            A.CallTo(() => streamNameResolver.WithNewId(A<string>.Ignored, A<Func<string, string>>.Ignored))
                .ReturnsLazily(new Func<string, Func<string, string>, string>((stream, idGenerator) => stream + "^2"));
        }

        [Theory]
        [InlineData(BackupVersion.V1)]
        [InlineData(BackupVersion.V2)]
        public async Task Should_write_and_read_events_to_backup(BackupVersion version)
        {
            var stream = new MemoryStream();

            var random = new Random();
            var randomGuids = new List<Guid>();

            for (var i = 0; i < 100; i++)
            {
                randomGuids.Add(Guid.NewGuid());
            }

            Guid RandomGuid()
            {
                return randomGuids[random.Next(randomGuids.Count)];
            }

            var sourceEvents = new List<(string Stream, Envelope<IEvent> Event)>();

            for (var i = 0; i < 200; i++)
            {
                var @event = new MyEvent
                {
                    GuidNamed = NamedId.Of(RandomGuid(), $"name{i}"),
                    GuidRaw = RandomGuid(),
                    Values = new Dictionary<Guid, string>
                    {
                        [RandomGuid()] = "Key"
                    }
                };

                var envelope = Envelope.Create<IEvent>(@event);

                envelope.Headers.Add(RandomGuid().ToString(), i);
                envelope.Headers.Add("Id", RandomGuid().ToString());
                envelope.Headers.Add("Index", i);

                sourceEvents.Add(($"My-{RandomGuid()}", envelope));
            }

            using (var writer = new BackupWriter(serializer, stream, true, version))
            {
                foreach (var @event in sourceEvents)
                {
                    var eventData = formatter.ToEventData(@event.Event, Guid.NewGuid(), true);
                    var eventStored = new StoredEvent("S", "1", 2, eventData);

                    var index = int.Parse(@event.Event.Headers["Index"].ToString());

                    if (index % 17 == 0)
                    {
                        await writer.WriteBlobAsync(index.ToString(), innerStream =>
                        {
                            innerStream.WriteByte((byte)index);

                            return TaskHelper.Done;
                        });
                    }
                    else if (index % 37 == 0)
                    {
                        await writer.WriteJsonAsync(index.ToString(), $"JSON_{index}");
                    }

                    writer.WriteEvent(eventStored);
                }
            }

            stream.Position = 0;

            var targetEvents = new List<(string Stream, Envelope<IEvent> Event)>();

            using (var reader = new BackupReader(serializer, stream))
            {
                await reader.ReadEventsAsync(streamNameResolver, formatter, async @event =>
                {
                    var index = int.Parse(@event.Event.Headers["Index"].ToString());

                    if (index % 17 == 0)
                    {
                        await reader.ReadBlobAsync(index.ToString(), innerStream =>
                        {
                            var byteRead = innerStream.ReadByte();

                            Assert.Equal((byte)index, byteRead);

                            return TaskHelper.Done;
                        });
                    }
                    else if (index % 37 == 0)
                    {
                        var json = await reader.ReadJsonAttachmentAsync<string>(index.ToString());

                        Assert.Equal($"JSON_{index}", json);
                    }

                    targetEvents.Add(@event);
                });

                void CompareGuid(Guid source, Guid target)
                {
                    Assert.Equal(source, reader.OldGuid(target));
                    Assert.NotEqual(source, target);
                }

                for (var i = 0; i < targetEvents.Count; i++)
                {
                    var source = targetEvents[i].Event.To<MyEvent>();

                    var target = sourceEvents[i].Event.To<MyEvent>();

                    CompareGuid(target.Payload.Values.First().Key, source.Payload.Values.First().Key);
                    CompareGuid(target.Payload.GuidRaw, source.Payload.GuidRaw);
                    CompareGuid(target.Payload.GuidNamed.Id, source.Payload.GuidNamed.Id);
                    CompareGuid(target.Headers.GetGuid("Id"), source.Headers.GetGuid("Id"));
                }
            }
        }
    }
}
