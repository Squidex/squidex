// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Hosting;
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
        public class GrainEnvironment
        {
            private AppContributors contributors = AppContributors.Empty;
            private long version = EtagVersion.Empty;

            public IGrainFactory GrainFactory { get; } = A.Fake<IGrainFactory>();

            public NamedId<DomainId> AppId { get; } = NamedId.Of(DomainId.NewGuid(), "my-app");

            public GrainEnvironment()
            {
                var indexGrain = A.Fake<IAppsByNameIndexGrain>();

                A.CallTo(() => indexGrain.GetIdAsync(AppId.Name))
                    .Returns(AppId.Id);

                var appGrain = A.Fake<IAppGrain>();

                A.CallTo(() => appGrain.GetStateAsync())
                    .ReturnsLazily(() => CreateApp().AsJ());

                A.CallTo(() => GrainFactory.GetGrain<IAppGrain>(AppId.Id.ToString(), null))
                    .Returns(appGrain);

                A.CallTo(() => GrainFactory.GetGrain<IAppsByNameIndexGrain>(SingleGrain.Id, null))
                    .Returns(indexGrain);
            }

            public void HandleCommand(CreateApp command)
            {
                version++;

                contributors = contributors.Assign(command.Actor.Identifier, Role.Developer);
            }

            public void HandleCommand(AssignContributor command)
            {
                version++;

                contributors = contributors.Assign(command.ContributorId, Role.Developer);
            }

            public void VerifyGrainAccess(int count)
            {
                A.CallTo(() => GrainFactory.GetGrain<IAppGrain>(AppId.Id.ToString(), null))
                    .MustHaveHappenedANumberOfTimesMatching(x => x == count);
            }

            private IAppEntity CreateApp()
            {
                var app = A.Fake<IAppEntity>();

                A.CallTo(() => app.Id)
                    .Returns(AppId.Id);

                A.CallTo(() => app.Name)
                    .Returns(AppId.Name);

                A.CallTo(() => app.Version)
                    .Returns(version);

                A.CallTo(() => app.Contributors)
                    .Returns(new AppContributors(contributors.ToDictionary()));

                return app;
            }
        }

        private sealed class Configurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.AddOrleansPubSub();
            }
        }

        [Theory]
        [InlineData(3, 100, 400, false)]
        [InlineData(3, 100, 202, true)]
        public async Task Should_distribute_and_cache_domain_objects(short numSilos, int numRuns, int expectedCounts, bool shouldBreak)
        {
            var env = new GrainEnvironment();

            var cluster =
                new TestClusterBuilder(numSilos)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            try
            {
                var indexes = GetIndexes(shouldBreak, env, cluster);

                var appId = env.AppId;

                var random = new Random();

                for (var i = 0; i < numRuns; i++)
                {
                    var contributorId = Guid.NewGuid().ToString();
                    var contributorCommand = new AssignContributor { ContributorId = contributorId, AppId = appId };

                    var commandContext = new CommandContext(contributorCommand, A.Fake<ICommandBus>());

                    var randomIndex = indexes[random.Next(numSilos)];

                    await randomIndex.HandleAsync(commandContext, x =>
                    {
                        if (x.Command is AssignContributor command)
                        {
                            env.HandleCommand(command);
                        }

                        x.Complete(true);

                        return Task.CompletedTask;
                    });

                    foreach (var index in indexes)
                    {
                        var appById = await index.GetAppAsync(appId.Id, true);
                        var appByName = await index.GetAppByNameAsync(appId.Name, true);

                        if (index == randomIndex || !shouldBreak || i == 0)
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

                env.VerifyGrainAccess(expectedCounts);
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }

        [Theory]
        [InlineData(3, false)]
        public async Task Should_retrieve_new_app(short numSilos, bool shouldBreak)
        {
            var env = new GrainEnvironment();

            var cluster =
                new TestClusterBuilder(numSilos)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            try
            {
                var indexes = GetIndexes(shouldBreak, env, cluster);

                var appId = env.AppId;

                foreach (var index in indexes)
                {
                    Assert.Null(await index.GetAppAsync(appId.Id, true));
                    Assert.Null(await index.GetAppByNameAsync(appId.Name, true));
                }

                var creatorId = Guid.NewGuid().ToString();
                var creatorToken = RefToken.User(creatorId);
                var createCommand = new CreateApp { Actor = creatorToken, AppId = appId.Id };

                var commandContext = new CommandContext(createCommand, A.Fake<ICommandBus>());

                var randomIndex = indexes[new Random().Next(3)];

                await indexes[0].HandleAsync(commandContext, x =>
                {
                    if (x.Command is CreateApp command)
                    {
                        env.HandleCommand(command);
                    }

                    x.Complete(true);

                    return Task.CompletedTask;
                });

                foreach (var index in indexes)
                {
                    var appById = await index.GetAppAsync(appId.Id, true);
                    var appByName = await index.GetAppByNameAsync(appId.Name, true);

                    if (index == randomIndex || !shouldBreak)
                    {
                        Assert.True(appById?.Contributors.ContainsKey(creatorId));
                        Assert.True(appByName?.Contributors.ContainsKey(creatorId));
                    }
                    else
                    {
                        Assert.False(appById?.Contributors.ContainsKey(creatorId));
                        Assert.False(appByName?.Contributors.ContainsKey(creatorId));
                    }
                }
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }

        private static AppsIndex[] GetIndexes(bool shouldBreak, GrainEnvironment env, TestCluster cluster)
        {
            return cluster.Silos.OfType<InProcessSiloHandle>()
                .Select(x =>
                {
                    var pubSub =
                        shouldBreak ?
                            A.Fake<IPubSub>() :
                            x.SiloHost.Services.GetRequiredService<IPubSub>();

                    var cache =
                        new ReplicatedCache(
                            new MemoryCache(Options.Create(new MemoryCacheOptions())),
                            pubSub,
                            Options.Create(new ReplicatedCacheOptions { Enable = true }));

                    return new AppsIndex(env.GrainFactory, cache);
                }).ToArray();
        }
    }
}
