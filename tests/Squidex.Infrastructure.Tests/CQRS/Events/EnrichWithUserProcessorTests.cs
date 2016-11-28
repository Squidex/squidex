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
    public class EnrichWithUserProcessorTests
    {
        public sealed class MyUserCommand : IUserCommand
        {
            public UserToken User { get; set; }
        }

        public sealed class MyNormalCommand : ICommand
        {
        }

        public sealed class MyEvent : IEvent
        {
        }

        private readonly EnrichWithUserProcessor sut = new EnrichWithUserProcessor();

        [Fact]
        public async Task Should_not_do_anything_if_not_user_command()
        {
            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, new MyNormalCommand());

            Assert.False(envelope.Headers.Contains("User"));
        }

        [Fact]
        public async Task Should_attach_user_to_event_envelope()
        {
            var user = new UserToken("subject", "123");
            var userCommand = new MyUserCommand { User = user };

            var envelope = new Envelope<IEvent>(new MyEvent());

            await sut.ProcessEventAsync(envelope, null, userCommand);

            Assert.Equal(user, envelope.Headers.User());
        }
    }
}
