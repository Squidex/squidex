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
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable SA1133 // Do not combine attributes

namespace Squidex.Infrastructure.Orleans
{
    public class GrainContextTests
    {
        public interface IContextGrain : IGrainWithStringKey
        {
            Task<GrainContext> TestAsync(GrainContext async);
        }

        public class ContextGrain : Grain, IContextGrain
        {
            public async Task<GrainContext> TestAsync(GrainContext context)
            {
                await Task.Delay(100);

                return context;
            }
        }

        [Fact]
        public void Should_capture_culture()
        {
            var culture = CultureInfo.GetCultureInfo("de");
            var cultureUI = CultureInfo.GetCultureInfo("it");

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = cultureUI;

            var sut = GrainContext.Create();

            Assert.Same(culture, sut.Culture);
            Assert.Same(cultureUI, sut.CultureUI);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var culture = CultureInfo.GetCultureInfo("de");
            var cultureUI = CultureInfo.GetCultureInfo("it");

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = cultureUI;

            var source = GrainContext.Create();
            var result = source.SerializeAndDeserializeBinary();

            Assert.Equal(culture, result.Culture);
            Assert.Equal(cultureUI, result.CultureUI);
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

            var source = GrainContext.Create();
            var result = await grain.TestAsync(source);

            Assert.Equal(culture, result.Culture);
            Assert.Equal(cultureUI, result.CultureUI);
        }
    }
}
