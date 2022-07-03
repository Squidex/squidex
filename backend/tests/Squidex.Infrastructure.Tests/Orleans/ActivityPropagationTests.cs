// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    [Trait("Category", "Dependencies")]
    public class ActivityPropagationTests
    {
        public interface IActivityGrain : IGrainWithStringKey
        {
            public Task<string> GetActivityId();
        }

        public class ActivityGrain : Grain, IActivityGrain
        {
            public Task<string> GetActivityId()
            {
                return Task.FromResult(Activity.Current?.TraceId.ToHexString() ?? string.Empty);
            }
        }

        public sealed class Configurator : ISiloConfigurator, IClientBuilderConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.AddIncomingGrainCallFilter<ActivityPropagationFilter>();
                siloBuilder.AddOutgoingGrainCallFilter<ActivityPropagationFilter>();
            }

            public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            {
                clientBuilder.AddOutgoingGrainCallFilter<ActivityPropagationFilter>();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_forward_activity(bool listen)
        {
            var cluster =
                new TestClusterBuilder(1)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .AddClientBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();
            try
            {
                using var listener = new ActivityListener
                {
                    ShouldListenTo = s => true,
                    Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) =>
                    {
                        return ActivitySamplingResult.AllData;
                    },
                    SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) =>
                    {
                        return ActivitySamplingResult.AllData;
                    }
                };

                if (listen)
                {
                    ActivitySource.AddActivityListener(listener);
                }

                using (var activity = Telemetry.Activities.StartActivity("Test", ActivityKind.Server))
                {
                    var grain = cluster.GrainFactory.GetGrain<IActivityGrain>(SingleGrain.Id);

                    var activityId = await grain.GetActivityId();

                    if (listen)
                    {
                        Assert.Equal(activity?.TraceId.ToHexString(), activityId);
                    }
                    else
                    {
                        Assert.Empty(activityId);
                    }
                }
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.DisposeAsync().AsTask());
            }
        }

        [Fact]
        public async Task Should_create_new_activity()
        {
            var cluster =
                new TestClusterBuilder(1)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .AddClientBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();
            try
            {
                using var listener = new ActivityListener
                {
                    ShouldListenTo = s => true,
                    Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) =>
                    {
                        return ActivitySamplingResult.AllData;
                    },
                    SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) =>
                    {
                        return ActivitySamplingResult.AllData;
                    }
                };

                ActivitySource.AddActivityListener(listener);

                var grain = cluster.GrainFactory.GetGrain<IActivityGrain>(SingleGrain.Id);

                var activityId = await grain.GetActivityId();

                Assert.NotEmpty(activityId);
            }
            finally
            {
                await Task.WhenAny(Task.Delay(2000), cluster.DisposeAsync().AsTask());
            }
        }
    }
}
