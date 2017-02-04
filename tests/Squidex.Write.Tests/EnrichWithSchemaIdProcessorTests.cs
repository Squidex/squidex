// ==========================================================================
//  EnrichWithSchemaIdProcessorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Core.Schemas;
using Squidex.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Write.Schemas;
using Xunit;

namespace Squidex.Write
{
    public class EnrichWithSchemaIdProcessorTests
    {
        public sealed class MySchemaCommand : SchemaAggregateCommand
        {
        }

        public sealed class MyNormalCommand : ICommand
        {
        }

        public sealed class MyEvent : IEvent
        {
        }

        private readonly EnrichWithSchemaIdProcessor sut = new EnrichWithSchemaIdProcessor();

        [Fact]
        public async Task Should_not_do_anything_if_not_app_command()
        {
            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, new MyNormalCommand());

            Assert.False(envelope.Headers.Contains("SchemaId"));
        }

        [Fact]
        public async Task Should_attach_app_id_from_domain_object()
        {
            var appId = Guid.NewGuid();

            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, new SchemaDomainObject(appId, 1, new FieldRegistry(new TypeNameRegistry())), new MyNormalCommand());

            Assert.Equal(appId, envelope.Headers.SchemaId());
        }

        [Fact]
        public async Task Should_attach_app_id_to_event_envelope()
        {
            var appId = Guid.NewGuid();
            var appCommand = new MySchemaCommand { AggregateId = appId };

            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, appCommand);

            Assert.Equal(appId, envelope.Headers.SchemaId());
        }
    }
}
