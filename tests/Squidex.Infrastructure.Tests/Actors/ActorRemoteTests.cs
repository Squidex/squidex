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

        private sealed class MyActor : IActor
        {
            private readonly SingleThreadedDispatcher dispatcher = new SingleThreadedDispatcher();

            public List<object> Invokes { get; } = new List<object>();

            public Task StopAndWaitAsync()
            {
                return dispatcher.StopAndWaitAsync();
            }

            public void Tell(object message)
            {
                dispatcher.DispatchAsync(() =>
                {
                    Invokes.Add(message);
                }).Forget();
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
        public async Task Should_handle_messages_sequentially()
        {
            remoteActor.Tell(new SuccessMessage { Counter = 1 });
            remoteActor.Tell(new SuccessMessage { Counter = 2 });
            remoteActor.Tell(new SuccessMessage { Counter = 3 });

            await actor.StopAndWaitAsync();

            actor.Invokes.ShouldBeEquivalentTo(new List<object>
            {
                new SuccessMessage { Counter = 1 },
                new SuccessMessage { Counter = 2 },
                new SuccessMessage { Counter = 3 }
            });
        }
    }
}
