// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class OrleansPubSub : IBackgroundProcess, IPubSub
    {
        private readonly IPubSubGrain pubSubGrain;
        private readonly IPubSubGrainObserver pubSubGrainObserver = new Observer();
        private readonly IGrainFactory grainFactory;

        private sealed class Observer : IPubSubGrainObserver
        {
            private readonly List<Action<object>> subscriptions = new List<Action<object>>();

            public void Handle(object message)
            {
                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        subscription(message);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            public void Subscribe(Action<object> handler)
            {
                subscriptions.Add(handler);
            }
        }

        public OrleansPubSub(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;

            pubSubGrain = grainFactory.GetGrain<IPubSubGrain>(SingleGrain.Id);
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var reference = await grainFactory.CreateObjectReference<IPubSubGrainObserver>(pubSubGrainObserver);

            await pubSubGrain.SubscribeAsync(reference);
        }

        public void Publish(object message)
        {
            Guard.NotNull(message, nameof(message));

            pubSubGrain.PublishAsync(message).Forget();
        }

        public void Subscribe(Action<object> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            pubSubGrainObserver.Subscribe(handler);
        }
    }
}
