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
            public DomainId DomainIdRaw { get; set; }

            public DomainId DomainIdEmpty { get; set; }

            public NamedId<DomainId> DomainIdNamed { get; set; }

            public Dictionary<DomainId, string> Values { get; set; }
        }

        public BackupReaderWriterTests()
        {
            typeNameRegistry.Map(typeof(MyEvent));

            formatter = new DefaultEventDataFormatter(typeNameRegistry, serializer);
        }

        [Theory]
        [InlineData(BackupVersion.V1)]
        [InlineData(BackupVersion.V2)]
        public async Task Should_write_and_read_events_to_backup(BackupVersion version)
        {
            var stream = new MemoryStream();

            var random = new Random();
            var randomDomainIds = new List<DomainId>();

            for (var i = 0; i < 100; i++)
            {
                randomDomainIds.Add(DomainId.NewGuid());
            }

            DomainId RandomDomainId()
            {
                return randomDomainIds[random.Next(randomDomainIds.Count)];
            }

            var sourceEvents = new List<(string Stream, Envelope<IEvent> Event)>();

            for (var i = 0; i < 200; i++)
            {
                var @event = new MyEvent
                {
                    DomainIdNamed = NamedId.Of(RandomDomainId(), $"name{i}"),
                    DomainIdRaw = RandomDomainId(),
                    Values = new Dictionary<DomainId, string>
                    {
                        [RandomDomainId()] = "Key"
                    }
                };

                var envelope = Envelope.Create<IEvent>(@event);

                envelope.Headers.Add(RandomDomainId().ToString(), i);
                envelope.Headers.Add("Id", RandomDomainId().ToString());
                envelope.Headers.Add("Index", i);

                sourceEvents.Add(($"My-{RandomDomainId()}", envelope));
            }

            using (var writer = new BackupWriter(serializer, stream, true, version))
            {
                foreach (var (_, envelope) in sourceEvents)
                {
                    var eventData = formatter.ToEventData(envelope, Guid.NewGuid(), true);
                    var eventStored = new StoredEvent("S", "1", 2, eventData);

                    var index = int.Parse(envelope.Headers["Index"].ToString());

                    if (index % 17 == 0)
                    {
                        await writer.WriteBlobAsync(index.ToString(), innerStream =>
                        {
                            innerStream.WriteByte((byte)index);

                            return Task.CompletedTask;
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

                            return Task.CompletedTask;
                        });
                    }
                    else if (index % 37 == 0)
                    {
                        var json = await reader.ReadJsonAttachmentAsync<string>(index.ToString());

                        Assert.Equal($"JSON_{index}", json);
                    }

                    targetEvents.Add(@event);
                });

                for (var i = 0; i < targetEvents.Count; i++)
                {
                    var target = targetEvents[i].Event.To<MyEvent>();

                    var source = sourceEvents[i].Event.To<MyEvent>();

                    Assert.Equal(source.Payload.Values.First().Key, target.Payload.Values.First().Key);
                    Assert.Equal(source.Payload.DomainIdRaw, target.Payload.DomainIdRaw);
                    Assert.Equal(source.Payload.DomainIdNamed.Id, target.Payload.DomainIdNamed.Id);

                    Assert.Equal(DomainId.Empty, target.Payload.DomainIdEmpty);
                }
            }
        }
    }
}
