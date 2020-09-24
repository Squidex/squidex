// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.TestingHost;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    [Trait("Category", "Dependencies")]
    public class PubSubTests
    {
        [Fact]
        public async Task Simple_pubsub_tests()
        {
            var cluster =
                new TestClusterBuilder(3)
                    .Build();

            await cluster.DeployAsync();

            var sent = new HashSet<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            var received1 = await CreateSubscriber(cluster.Client, sent.Count);
            var received2 = await CreateSubscriber(cluster.Client, sent.Count);

            var pubSub = new OrleansPubSub(cluster.Client);

            foreach (var message in sent)
            {
                pubSub.Publish(message);
            }

            await Task.WhenAny(
                Task.WhenAll(
                    received1,
                    received2
                ),
                Task.Delay(10000));

            Assert.True(received1.Result.SetEquals(sent));
            Assert.True(received2.Result.SetEquals(sent));
        }

        private static async Task<Task<HashSet<Guid>>> CreateSubscriber(IGrainFactory grainFactory, int expectedCount)
        {
            var pubSub = new OrleansPubSub(grainFactory);

            await pubSub.StartAsync(default);

            var received = new HashSet<Guid>();
            var receivedCompleted = new TaskCompletionSource<HashSet<Guid>>();

            pubSub.Subscribe(message =>
            {
                if (message is Guid guid)
                {
                    received.Add(guid);
                }

                if (received.Count == expectedCount)
                {
                    receivedCompleted.TrySetResult(received);
                }
            });

            return receivedCompleted.Task;
        }
    }
}
