// ==========================================================================
//  EnrichWithUserProcessorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Commands;
using Xunit;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class EnrichWithActorProcessorTests
    {
        public sealed class MyActorCommand : IActorCommand
        {
            public RefToken Actor { get; set; }
        }

        public sealed class MyNormalCommand : ICommand
        {
        }

        public sealed class MyEvent : IEvent
        {
        }

        private readonly EnrichWithActorProcessor sut = new EnrichWithActorProcessor();

        [Fact]
        public async Task Should_not_do_anything_if_not_actor_command()
        {
            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, new MyNormalCommand());

            Assert.False(envelope.Headers.Contains("User"));
        }

        [Fact]
        public async Task Should_attach_user_to_event_envelope()
        {
            var actorToken = new RefToken("subject", "123");
            var actorCommand = new MyActorCommand { Actor = actorToken };

            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, actorCommand);

            Assert.Equal(actorToken, envelope.Headers.Actor());
        }
    }
}
