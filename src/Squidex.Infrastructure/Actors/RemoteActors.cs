// ==========================================================================
//  RemoteActors.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Concurrent;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Actors
{
    public sealed class RemoteActors : IActors
    {
        private readonly ConcurrentDictionary<string, IActor> senders = new ConcurrentDictionary<string, IActor>();
        private readonly ConcurrentDictionary<string, bool> receivers = new ConcurrentDictionary<string, bool>();
        private readonly IRemoteActorChannel channel;

        private sealed class Sender : IActor
        {
            private readonly IRemoteActorChannel channel;
            private readonly string recipient;

            public Sender(IRemoteActorChannel channel, string recipient)
            {
                this.recipient = recipient;

                this.channel = channel;
            }

            public void Tell(object message)
            {
                channel.SendAsync(recipient, message).Forget();
            }
        }

        public RemoteActors(IRemoteActorChannel channel)
        {
            Guard.NotNull(channel, nameof(channel));

            this.channel = channel;
        }

        public IActor Get(string id)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return senders.GetOrAdd(id, k => new Sender(channel, id));
        }

        public void Connect(string id, IActor actor)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(actor, nameof(actor));

            channel.Subscribe(id, actor.Tell);
        }
    }
}
