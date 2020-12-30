// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.TestingHost;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    [Trait("Category", "Dependencies")]
    public class AsyncLocalTests
    {
        public interface IAsyncLocalGrain : IGrainWithStringKey
        {
            public Task<int> GetValueAsync();
        }

        public class AsyncLocalGrain : Grain, IAsyncLocalGrain
        {
            private readonly AsyncLocal<int> temp = new AsyncLocal<int>();

            public Task<int> GetValueAsync()
            {
                temp.Value++;

                return Task.FromResult(temp.Value);
            }
        }

        [Fact]
        public async Task Should_use_async_local()
        {
            var cluster =
                new TestClusterBuilder(1)
                    .Build();

            await cluster.DeployAsync();

            try
            {
                var grain = cluster.GrainFactory.GetGrain<IAsyncLocalGrain>(SingleGrain.Id);

                var result1 = await grain.GetValueAsync();
                var result2 = await grain.GetValueAsync();

                await cluster.KillSiloAsync(cluster.Silos[0]);
                await cluster.StartAdditionalSiloAsync();

                var result3 = await grain.GetValueAsync();

                Assert.Equal(1, result1);
                Assert.Equal(1, result2);
                Assert.Equal(1, result3);
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }
    }
}
