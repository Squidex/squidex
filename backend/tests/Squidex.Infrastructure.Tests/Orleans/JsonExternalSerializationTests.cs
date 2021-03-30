// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
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
            public Task<J<CommandResult>> ExecuteAsync(J<CommandRequest> request);
        }

        public class CommandGrain : Grain, ICommandGrain
        {
            public Task<J<CommandResult>> ExecuteAsync(J<CommandRequest> request)
            {
                request.Value.ApplyContext();

                var command = (TestCommand)request.Value.Command;

                var result = new CommandResult(command.AggregateId, 0, 0, command.Value);

                return Task.FromResult(result.AsJ());
            }
        }

        public class TestCommand : IAggregateCommand
        {
            public DomainId AggregateId { get; set; }

            public long ExpectedVersion { get; set; }

            public string Value { get; set; }
        }

        public JsonExternalSerializationTests()
        {
            J.DefaultSerializer = TestUtils.DefaultSerializer;
        }

        [Fact]
        public async Task Should_make_request_with_json_serializer()
        {
            var cluster =
                new TestClusterBuilder(1)
                    .Build();

            await cluster.DeployAsync();

            try
            {
                for (var i = 0; i < 100; i++)
                {
                    var id = DomainId.NewGuid().ToString();

                    var grain = cluster.GrainFactory.GetGrain<ICommandGrain>(id);

                    var result = await grain.ExecuteAsync(CommandRequest.Create(new TestCommand
                    {
                        Value = id
                    }));

                    Assert.Equal(id, result.Value.Payload);
                }
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }
    }
}
