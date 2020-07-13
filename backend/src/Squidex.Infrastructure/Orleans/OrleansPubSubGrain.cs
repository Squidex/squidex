// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public sealed class OrleansPubSubGrain : Grain, IPubSubGrain
    {
        private readonly List<IPubSubGrainObserver> subscriptions = new List<IPubSubGrainObserver>();

        public Task PublishAsync(object message)
        {
            foreach (var subscription in subscriptions)
            {
                try
                {
                    subscription.Handle(message);
                }
                catch
                {
                    continue;
                }
            }

            return Task.CompletedTask;
        }

        public Task SubscribeAsync(IPubSubGrainObserver observer)
        {
            subscriptions.Add(observer);

            return Task.CompletedTask;
        }
    }
}
