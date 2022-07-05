// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    [Trait("Category", "Dependencies")]
    public class JsonExternalSerializationTests
    {
        public interface ICommandGrain : IGrainWithStringKey
        {
            public Task<CommandResult> ExecuteAsync(IAggregateCommand command);
        }

        public class CommandGrain : Grain, ICommandGrain
        {
            public Task<CommandResult> ExecuteAsync(IAggregateCommand command)
            {
                var result = new CommandResult(command.AggregateId, 0, 0, ((TestCommand)command).Value);

                return Task.FromResult(result);
            }
        }

        public class TestCommand : IAggregateCommand
        {
            public DomainId AggregateId { get; set; }

            public long ExpectedVersion { get; set; }

            public string Value { get; set; }
        }

        public sealed class Configurator : ISiloConfigurator, IClientBuilderConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(TestUtils.DefaultSerializer);
                });

                siloBuilder.Configure<SerializationProviderOptions>(options =>
                {
                    options.SerializationProviders.Add(typeof(JsonSerializer));
                });
            }

            public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            {
                clientBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(TestUtils.DefaultSerializer);
                });

                clientBuilder.Configure<SerializationProviderOptions>(options =>
                {
                    options.SerializationProviders.Add(typeof(JsonSerializer));
                });
            }
        }

        [Fact]
        public async Task Should_make_request_with_json_serializer()
        {
            var cluster =
                new TestClusterBuilder(1)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .AddClientBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            try
            {
                for (var i = 0; i < 100; i++)
                {
                    var id = DomainId.NewGuid().ToString();

                    var commandGrain = cluster.GrainFactory.GetGrain<ICommandGrain>(id);
                    var commandTest = new TestCommand { Value = id };

                    var result = await commandGrain.ExecuteAsync(commandTest);

                    Assert.Equal(id, result.Payload);
                }
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }
    }
}
