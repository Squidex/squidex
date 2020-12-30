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
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes
{
    [Trait("Category", "Dependencies")]
    public class SchemasIndexIntegrationTests
    {
        private static GrainRuntime currentRuntime;

        public class GrainRuntime
        {
            private Schema schema = new Schema("my-schema");

            public IGrainFactory GrainFactory { get; } = A.Fake<IGrainFactory>();

            public NamedId<DomainId> AppId { get; } = NamedId.Of(DomainId.NewGuid(), "my-app");

            public NamedId<DomainId> SchemaId { get; } = NamedId.Of(DomainId.NewGuid(), "my-schema");

            public bool ShouldBreak { get; set; }

            public GrainRuntime()
            {
                var indexGrain = A.Fake<ISchemasByAppIndexGrain>();

                A.CallTo(() => indexGrain.GetIdAsync(AppId.Name))
                    .Returns(AppId.Id);

                var schemaGrain = A.Fake<ISchemaGrain>();

                A.CallTo(() => schemaGrain.GetStateAsync())
                    .ReturnsLazily(() => CreateEntity().AsJ());

                A.CallTo(() => GrainFactory.GetGrain<ISchemaGrain>(DomainId.Combine(AppId.Id, SchemaId.Id).ToString(), null))
                    .Returns(schemaGrain);

                A.CallTo(() => GrainFactory.GetGrain<ISchemasByAppIndexGrain>(SingleGrain.Id, null))
                    .Returns(indexGrain);
            }

            public void HandleCommand(AddField addField)
            {
                schema = schema.AddString(schema.Fields.Count + 1, addField.Name, Partitioning.Invariant);
            }

            public void VerifyGrainAccess(int count)
            {
                A.CallTo(() => GrainFactory.GetGrain<ISchemaGrain>(DomainId.Combine(AppId.Id, SchemaId.Id).ToString(), null))
                    .MustHaveHappenedANumberOfTimesMatching(x => x == count);
            }

            private ISchemaEntity CreateEntity()
            {
                var appEntity = A.Fake<ISchemaEntity>();

                A.CallTo(() => appEntity.Id)
                    .Returns(currentRuntime.SchemaId.Id);

                A.CallTo(() => appEntity.AppId)
                    .Returns(currentRuntime.AppId);

                A.CallTo(() => appEntity.SchemaDef)
                    .Returns(schema);

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

            public SchemasIndex Index { get; }

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

                Index = new SchemasIndex(currentRuntime.GrainFactory, cache);
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

                var schemaId = currentRuntime.SchemaId;

                var random = new Random();

                for (var i = 0; i < numRuns; i++)
                {
                    var fieldName = Guid.NewGuid().ToString();
                    var fieldCommand = new AddField { Name = fieldName, SchemaId = schemaId, AppId = appId };

                    var commandContext = new CommandContext(fieldCommand, A.Fake<ICommandBus>());

                    var randomSilo = SiloHandle.All.ElementAt(random.Next(numSilos));

                    await randomSilo.Index.HandleAsync(commandContext, x =>
                    {
                        if (x.Command is AddField command)
                        {
                            currentRuntime.HandleCommand(command);
                        }

                        x.Complete(true);

                        return Task.CompletedTask;
                    });

                    foreach (var silo in SiloHandle.All)
                    {
                        var schemaById = await silo.Index.GetSchemaAsync(appId.Id, schemaId.Id, true);
                        var schemaByName = await silo.Index.GetSchemaByNameAsync(appId.Id, schemaId.Name, true);

                        if (silo == randomSilo || !currentRuntime.ShouldBreak || i == 0)
                        {
                            Assert.True(schemaById?.SchemaDef.FieldsByName.ContainsKey(fieldName));
                            Assert.True(schemaByName?.SchemaDef.FieldsByName.ContainsKey(fieldName));
                        }
                        else
                        {
                            Assert.False(schemaById?.SchemaDef.FieldsByName.ContainsKey(fieldName));
                            Assert.False(schemaByName?.SchemaDef.FieldsByName.ContainsKey(fieldName));
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
