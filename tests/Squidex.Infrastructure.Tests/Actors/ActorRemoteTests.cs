// ==========================================================================
//  ActorRemoteTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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
        public class SuccessMessage : object
        {
            public int Counter { get; set; }
        }

        private sealed class MyActor : Actor, IActor
        {
            public List<object> Invokes { get; } = new List<object>();

            public void Tell(object message)
            {
                DispatchAsync(message).Forget();
            }

            protected override Task OnMessage(object message)
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
        public void Should_handle_messages_sequentially()
        {
            remoteActor.Tell(new SuccessMessage { Counter = 1 });
            remoteActor.Tell(new SuccessMessage { Counter = 2 });
            remoteActor.Tell(new SuccessMessage { Counter = 3 });

            actor.Dispose();

            actor.Invokes.ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 1 },
                new SuccessMessage { Counter = 2 },
                new SuccessMessage { Counter = 3 }
            });
        }
    }
}
