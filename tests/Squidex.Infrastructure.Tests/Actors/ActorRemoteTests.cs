// ==========================================================================
//  ActorRemoteTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Actors
{
    public class ActorRemoteTests
    {
        [TypeName(nameof(SuccessMessage))]
        public class SuccessMessage : IMessage
        {
            public int Counter { get; set; }
        }

        private sealed class MyActor : Actor
        {
            public List<IMessage> Invokes { get; } = new List<IMessage>();

            protected override Task OnMessage(IMessage message)
            {
                Invokes.Add(message);

                return TaskHelper.Done;
            }
        }

        private readonly MyActor actor = new MyActor();
        private readonly TypeNameRegistry registry = new TypeNameRegistry();
        private readonly RemoteActors actors;
        private readonly IActor remoteActor;

        public ActorRemoteTests()
        {
            registry.Map(typeof(SuccessMessage));

            actors = new RemoteActors(new DefaultRemoteActorChannel(new InMemoryPubSub(), registry));
            actors.Connect("my", actor);

            remoteActor = actors.Get("my");
        }

        [Fact]
        public void Should_throw_exception_when_stopping_remote_actor()
        {
            Assert.Throws<NotSupportedException>(() => remoteActor.StopAsync().Forget());
        }

        [Fact]
        public void Should_throw_exception_when_sending_exception_to_remote_actor()
        {
            Assert.Throws<NotSupportedException>(() => remoteActor.SendAsync(new InvalidOperationException()).Forget());
        }

        [Fact]
        public async Task Should_handle_messages_sequentially()
        {
            remoteActor.SendAsync(new SuccessMessage { Counter = 1 }).Forget();
            remoteActor.SendAsync(new SuccessMessage { Counter = 2 }).Forget();
            remoteActor.SendAsync(new SuccessMessage { Counter = 3 }).Forget();

            await actor.StopAsync();

            actor.Invokes.ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 1 },
                new SuccessMessage { Counter = 2 },
                new SuccessMessage { Counter = 3 }
            });
        }
    }
}
