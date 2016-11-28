// ==========================================================================
//  EnrichWithAggregateIdProcessorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Commands;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class EnrichWithAggregateIdProcessorTests
    {
        public sealed class MyAggregateIdCommand : IAggregateCommand
        {
            public Guid AggregateId { get; set; }
        }

        public sealed class MyNormalCommand : ICommand
        {
        }

        public sealed class MyEvent : IEvent
        {
        }

        private readonly EnrichWithAggregateIdProcessor sut = new EnrichWithAggregateIdProcessor();

        [Fact]
        public async Task Should_not_do_anything_if_not_aggregate_command()
        {
            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, new MyNormalCommand());

            Assert.False(envelope.Headers.Contains("AggregateId"));
        }

        [Fact]
        public async Task Should_attach_aggregate_to_event_envelope()
        {
            var aggregateId = Guid.NewGuid();
            var aggregateCommand = new MyAggregateIdCommand { AggregateId = aggregateId };

            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, aggregateCommand);

            Assert.Equal(aggregateId, envelope.Headers.AggregateId());
        }
    }
}
