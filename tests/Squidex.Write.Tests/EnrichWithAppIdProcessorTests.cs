// ==========================================================================
//  EnrichWithAppIdProcessorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Events;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Write.Apps;
using Xunit;

namespace Squidex.Write
{
    public class EnrichWithAppIdProcessorTests
    {
        public sealed class MyAppCommand : AppAggregateCommand
        {
        }

        public sealed class MyNormalCommand : ICommand
        {
        }

        public sealed class MyEvent : IEvent
        {
        }

        private readonly EnrichWithAppIdProcessor sut = new EnrichWithAppIdProcessor();

        [Fact]
        public async Task Should_not_do_anything_if_not_app_command()
        {
            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, new MyNormalCommand());

            Assert.False(envelope.Headers.Contains("AppId"));
        }

        [Fact]
        public async Task Should_attach_app_id_from_domain_object()
        {
            var appId = Guid.NewGuid();

            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, new AppDomainObject(appId, 1), new MyNormalCommand());

            Assert.Equal(appId, envelope.Headers.AppId());
        }

        [Fact]
        public async Task Should_attach_app_id_to_event_envelope()
        {
            var appId = Guid.NewGuid();
            var appCommand = new MyAppCommand { AggregateId = appId };

            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, appCommand);

            Assert.Equal(appId, envelope.Headers.AppId());
        }
    }
}
