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
        public class GrainEnvironment
        {
            private Schema schema = new Schema("my-schema");
            private long version = EtagVersion.Empty;

            public IGrainFactory GrainFactory { get; } = A.Fake<IGrainFactory>();

            public NamedId<DomainId> AppId { get; } = NamedId.Of(DomainId.NewGuid(), "my-app");

            public NamedId<DomainId> SchemaId { get; } = NamedId.Of(DomainId.NewGuid(), "my-schema");

            public GrainEnvironment()
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

            public void HandleCommand(AddField command)
            {
                version++;

                schema = schema.AddString(schema.Fields.Count + 1, command.Name, Partitioning.Invariant);
            }

            public void VerifyGrainAccess(int count)
            {
                A.CallTo(() => GrainFactory.GetGrain<ISchemaGrain>(DomainId.Combine(AppId.Id, SchemaId.Id).ToString(), null))
                    .MustHaveHappenedANumberOfTimesMatching(x => x == count);
            }

            private ISchemaEntity CreateEntity()
            {
                var schemaEntity = A.Fake<ISchemaEntity>();

                A.CallTo(() => schemaEntity.Id)
                    .Returns(SchemaId.Id);

                A.CallTo(() => schemaEntity.AppId)
                    .Returns(AppId);

                A.CallTo(() => schemaEntity.Version)
                    .Returns(version);

                A.CallTo(() => schemaEntity.SchemaDef)
                    .Returns(schema);

                return schemaEntity;
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
                var indexes =
                    cluster.Silos.OfType<InProcessSiloHandle>()
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

                            return new SchemasIndex(env.GrainFactory, cache);
                        }).ToArray();

                var appId = env.AppId;

                var random = new Random();

                for (var i = 0; i < numRuns; i++)
                {
                    var fieldName = Guid.NewGuid().ToString();
                    var fieldCommand = new AddField { Name = fieldName, SchemaId = env.SchemaId, AppId = env.AppId };

                    var commandContext = new CommandContext(fieldCommand, A.Fake<ICommandBus>());

                    var randomIndex = indexes[random.Next(numSilos)];

                    await randomIndex.HandleAsync(commandContext, x =>
                    {
                        if (x.Command is AddField command)
                        {
                            env.HandleCommand(command);
                        }

                        x.Complete(true);

                        return Task.CompletedTask;
                    });

                    foreach (var index in indexes)
                    {
                        var schemaById = await index.GetSchemaAsync(appId.Id, env.SchemaId.Id, true);
                        var schemaByName = await index.GetSchemaByNameAsync(appId.Id, env.SchemaId.Name, true);

                        if (index == randomIndex || !shouldBreak || i == 0)
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

                env.VerifyGrainAccess(expectedCounts);
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }
    }
}
