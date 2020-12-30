// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.TestingHost;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    [Trait("Category", "Dependencies")]
    public class AppsIndexIntegrationTests
    {
        private static GrainRuntime currentRuntime;

        public class GrainRuntime
        {
            private AppContributors contributors = AppContributors.Empty;

            public IGrainFactory GrainFactory { get; } = A.Fake<IGrainFactory>();

            public NamedId<DomainId> AppId { get; } = NamedId.Of(DomainId.NewGuid(), "my-app");

            public bool ShouldBreak { get; set; }

            public GrainRuntime()
            {
                var indexGrain = A.Fake<IAppsByNameIndexGrain>();

                A.CallTo(() => indexGrain.GetIdAsync(AppId.Name))
                    .Returns(AppId.Id);

                var appGrain = A.Fake<IAppGrain>();

                A.CallTo(() => appGrain.GetStateAsync())
                    .ReturnsLazily(() => CreateEntity().AsJ());

                A.CallTo(() => GrainFactory.GetGrain<IAppGrain>(AppId.Id.ToString(), null))
                    .Returns(appGrain);

                A.CallTo(() => GrainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id, null))
                    .Returns(indexGrain);
            }

            public void HandleCommand(AssignContributor contributor)
            {
                contributors = contributors.Assign(contributor.ContributorId, Role.Developer);
            }

            public void VerifyGrainAccess(int count)
            {
                A.CallTo(() => GrainFactory.GetGrain<IAppGrain>(AppId.Id.ToString(), null))
                    .MustHaveHappenedANumberOfTimesMatching(x => x == count);
            }

            private IAppEntity CreateEntity()
            {
                var appEntity = A.Fake<IAppEntity>();

                A.CallTo(() => appEntity.Id)
                    .Returns(currentRuntime.AppId.Id);

                A.CallTo(() => appEntity.Name)
                    .Returns(currentRuntime.AppId.Name);

                A.CallTo(() => appEntity.Contributors)
                    .Returns(new AppContributors(contributors.ToDictionary()));

                return appEntity;
            }
        }

        private sealed class Configurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.AddOrleansPubSub();
                siloBuilder.AddStartupTask<SiloHandle>();
            }
        }

        private class NoopPubSub : IPubSub
        {
            public Task PublishAsync(object? payload)
            {
                return Task.CompletedTask;
            }

            public Task SubscribeAsync(Action<object?> subscriber)
            {
                return Task.CompletedTask;
            }
        }

        protected sealed class SiloHandle : IStartupTask, IDisposable
        {
            private static readonly ConcurrentDictionary<SiloHandle, SiloHandle> AllSilos = new ConcurrentDictionary<SiloHandle, SiloHandle>();

            public AppsIndex Index { get; }

            public static ICollection<SiloHandle> All => AllSilos.Keys;

            public SiloHandle(IPubSub pubSub)
            {
                if (currentRuntime.ShouldBreak)
                {
                    pubSub = new NoopPubSub();
                }

                var cache =
                    new ReplicatedCache(
                        new MemoryCache(Options.Create(new MemoryCacheOptions())),
                        pubSub,
                        Options.Create(new ReplicatedCacheOptions { Enable = true }));

                Index = new AppsIndex(currentRuntime.GrainFactory, cache);
            }

            public static void Clear()
            {
                AllSilos.Clear();
            }

            public Task Execute(CancellationToken cancellationToken)
            {
                AllSilos.TryAdd(this, this);

                return Task.CompletedTask;
            }

            public void Dispose()
            {
                AllSilos.TryRemove(this, out _);
            }
        }

        [Theory]
        [InlineData(3, 100, 300, false)]
        [InlineData(3, 100, 102, true)]
        public async Task Should_distribute_and_cache_domain_objects(short numSilos, int numRuns, int expectedCounts, bool shouldBreak)
        {
            currentRuntime = new GrainRuntime { ShouldBreak = shouldBreak };

            var cluster =
                new TestClusterBuilder(numSilos)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            try
            {
                var appId = currentRuntime.AppId;

                var random = new Random();

                for (var i = 0; i < numRuns; i++)
                {
                    var contributorId = Guid.NewGuid().ToString();
                    var contributorCommand = new AssignContributor { ContributorId = contributorId, AppId = appId };

                    var commandContext = new CommandContext(contributorCommand, A.Fake<ICommandBus>());

                    var randomSilo = SiloHandle.All.ElementAt(random.Next(numSilos));

                    await randomSilo.Index.HandleAsync(commandContext, x =>
                    {
                        if (x.Command is AssignContributor command)
                        {
                            currentRuntime.HandleCommand(command);
                        }

                        x.Complete(true);

                        return Task.CompletedTask;
                    });

                    foreach (var silo in SiloHandle.All)
                    {
                        var appById = await silo.Index.GetAppAsync(appId.Id, true);
                        var appByName = await silo.Index.GetAppByNameAsync(appId.Name, true);

                        if (silo == randomSilo || !currentRuntime.ShouldBreak || i == 0)
                        {
                            Assert.True(appById?.Contributors.ContainsKey(contributorId));
                            Assert.True(appByName?.Contributors.ContainsKey(contributorId));
                        }
                        else
                        {
                            Assert.False(appById?.Contributors.ContainsKey(contributorId));
                            Assert.False(appByName?.Contributors.ContainsKey(contributorId));
                        }
                    }
                }

                currentRuntime.VerifyGrainAccess(expectedCounts);
            }
            finally
            {
                SiloHandle.Clear();

                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }
    }
}
