// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Threading.Tasks;
using Orleans;
using Orleans.TestingHost;
using Xunit;

#pragma warning disable SA1133 // Do not combine attributes

namespace Squidex.Infrastructure.Commands
{
    public class CommandRequestTests
    {
        public interface IContextGrain : IGrainWithStringKey
        {
            Task<string> GetCultureUIAsync(CommandRequest request);

            Task<string> GetCultureAsync(CommandRequest request);
        }

        public class ContextGrain : Grain, IContextGrain
        {
            public Task<string> GetCultureAsync(CommandRequest request)
            {
                request.ApplyContext();

                return Task.FromResult(CultureInfo.CurrentCulture.Name);
            }

            public Task<string> GetCultureUIAsync(CommandRequest request)
            {
                request.ApplyContext();

                return Task.FromResult(CultureInfo.CurrentUICulture.Name);
            }
        }

        [Fact]
        public void Should_capture_culture()
        {
            var culture = CultureInfo.GetCultureInfo("de");
            var cultureUI = CultureInfo.GetCultureInfo("it");

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = cultureUI;

            var sut = CommandRequest.Create(null!);

            Assert.Equal(culture.Name, sut.Culture);
            Assert.Equal(cultureUI.Name, sut.CultureUI);
        }

        [Fact, Trait("Category", "Dependencies")]
        public async Task Should_communicate_with_orleans()
        {
            var cluster =
                new TestClusterBuilder(1)
                    .Build();

            await cluster.DeployAsync();

            var culture = CultureInfo.GetCultureInfo("de");
            var cultureUI = CultureInfo.GetCultureInfo("it");

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = cultureUI;

            var grain = cluster.Client.GetGrain<IContextGrain>("Default");

            var request = CommandRequest.Create(null!);

            var cultureFromGrain = await grain.GetCultureAsync(request);
            var cultureUIFromGrain = await grain.GetCultureUIAsync(request);

            Assert.Equal(culture.Name, cultureFromGrain);
            Assert.Equal(cultureUI.Name, cultureUIFromGrain);
        }
    }
}
