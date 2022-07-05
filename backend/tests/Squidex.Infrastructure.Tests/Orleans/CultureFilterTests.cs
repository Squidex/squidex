// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    [Trait("Category", "Dependencies")]
    public class CultureFilterTests
    {
        public interface ICultureGrain : IGrainWithStringKey
        {
            public Task<string> GetCultureAsync(bool chaining);

            public Task<string> GetUICultureAsync(bool chaining);
        }

        public class CultureGrain : Grain, ICultureGrain
        {
            public Task<string> GetCultureAsync(bool chaining)
            {
                if (chaining)
                {
                    return GrainFactory.GetGrain<ICultureGrain>("1").GetCultureAsync(false);
                }

                return Task.FromResult(CultureInfo.CurrentCulture.ToString());
            }

            public Task<string> GetUICultureAsync(bool chaining)
            {
                if (chaining)
                {
                    return GrainFactory.GetGrain<ICultureGrain>("1").GetUICultureAsync(false);
                }

                return Task.FromResult(CultureInfo.CurrentUICulture.ToString());
            }
        }

        public sealed class Configurator : ISiloConfigurator, IClientBuilderConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.AddIncomingGrainCallFilter<CultureFilter>();
                siloBuilder.AddOutgoingGrainCallFilter<CultureFilter>();
            }

            public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
            {
                clientBuilder.AddOutgoingGrainCallFilter<CultureFilter>();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_forward_culture(bool chaining)
        {
            await using var cluster =
                new TestClusterBuilder(1)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .AddClientBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            var previousCulture = CultureInfo.CurrentCulture;
            var previousUICulture = CultureInfo.CurrentUICulture;
            try
            {
                var culture = CultureInfo.GetCultureInfo("de");
                var cultureUI = CultureInfo.GetCultureInfo("it");

                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = cultureUI;

                var grain = cluster.GrainFactory.GetGrain<ICultureGrain>(SingleGrain.Id);

                var cultureFromGrain = await grain.GetCultureAsync(chaining);
                var cultureUIFromGrain = await grain.GetUICultureAsync(chaining);

                Assert.Equal(culture.Name, cultureFromGrain);
                Assert.Equal(cultureUI.Name, cultureUIFromGrain);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUICulture;
            }
        }
    }
}
