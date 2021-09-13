// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable SA1133 // Do not combine attributes

namespace Squidex.Infrastructure.Orleans
{
    public class ExceptionWrapperFilterTests
    {
        private readonly IIncomingGrainCallContext context = A.Fake<IIncomingGrainCallContext>();
        private readonly ExceptionWrapperFilter sut;

        public interface IExceptionGrain : IGrainWithStringKey
        {
            Task ThrowCustomAsync();

            Task ThrowMongoAsync();
        }

        public sealed class ExceptionGrain : Grain, IExceptionGrain
        {
            public Task ThrowCustomAsync()
            {
                throw new InvalidException("My Message");
            }

            public Task ThrowMongoAsync()
            {
                var connection = new ConnectionId(new ServerId(new ClusterId(), new IPEndPoint(IPAddress.Loopback, 21017)), 1);

                throw new MongoWriteException(connection, null, null, null);
            }
        }

        private sealed class InvalidException : Exception
        {
            public InvalidException(string message)
                : base(message)
            {
            }
        }

        public sealed class Configurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.AddIncomingGrainCallFilter<ExceptionWrapperFilter>();
            }
        }

        public ExceptionWrapperFilterTests()
        {
            sut = new ExceptionWrapperFilter();
        }

        [Fact]
        public async Task Should_just_forward_serializable_exception()
        {
            var original = new InvalidOperationException();

            A.CallTo(() => context.Invoke())
                .Throws(original);

            var ex = await Assert.ThrowsAnyAsync<Exception>(() => sut.Invoke(context));

            Assert.Same(ex, original);
        }

        [Fact]
        public async Task Should_wrap_non_serializable_exception()
        {
            var original = new InvalidException("My Message");

            A.CallTo(() => context.Invoke())
                .Throws(original);

            var ex = await Assert.ThrowsAnyAsync<OrleansWrapperException>(() => sut.Invoke(context));

            Assert.Equal(original.GetType(), ex.ExceptionType);
            Assert.Contains(original.Message, ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var original = new InvalidException("My Message");

            var source = new OrleansWrapperException(original, original.GetType());
            var serialized = source.SerializeAndDeserializeBinary();

            Assert.Equal(serialized.ExceptionType, source.ExceptionType);
            Assert.Equal(serialized.Message, source.Message);
        }

        [Fact, Trait("Category", "Dependencies")]
        public async Task Simple_grain_tests()
        {
            var (cluster, grain) = await GetGrainAsync();

            try
            {
                var ex = await Assert.ThrowsAsync<OrleansWrapperException>(() => grain.ThrowCustomAsync());

                Assert.Equal(typeof(InvalidException), ex.ExceptionType);
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }

        [Fact, Trait("Category", "Dependencies")]
        public async Task Simple_grain_tests_with_mongo_exception()
        {
            var (cluster, grain) = await GetGrainAsync();

            try
            {
                var ex = await Assert.ThrowsAsync<OrleansWrapperException>(() => grain.ThrowMongoAsync());

                Assert.Equal(typeof(MongoWriteException), ex.ExceptionType);
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.StopAllSilosAsync());
            }
        }

        private static async Task<(TestCluster, IExceptionGrain)> GetGrainAsync()
        {
            var cluster =
                new TestClusterBuilder(1)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            return (cluster, cluster.GrainFactory.GetGrain<IExceptionGrain>(SingleGrain.Id));
        }
    }
}
