// ==========================================================================
//  CachingAppProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Infrastructure;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Apps.Services.Implementations;
using Squidex.Read.MongoDb.Apps;
using Xunit;

// ReSharper disable ConvertToConstant.Local
// ReSharper disable UnusedParameter.Local

namespace Squidex.Read.Apps
{
    public class CachingAppProviderTests
    {
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly Mock<IAppRepository> repository = new Mock<IAppRepository>();
        private readonly CachingAppProvider sut;
        private readonly MongoAppEntity appV1;
        private readonly MongoAppEntity appV2;
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");

        public CachingAppProviderTests()
        {
            appV1 = new MongoAppEntity { Name = appId.Name, Id = appId.Id };
            appV2 = new MongoAppEntity { Name = appId.Name, Id = appId.Id };

            sut = new CachingAppProvider(cache, repository.Object);
        }

        [Fact]
        public async Task Should_also_retrieve_app_by_name_if_retrieved_by_id_before()
        {
            repository.Setup(x => x.FindAppAsync(appId.Id)).Returns(Task.FromResult<IAppEntity>(appV1));

            await ProvideAppById(appV1);
            await ProvideAppByName(appV1);

            repository.Verify(x => x.FindAppAsync(appId.Id), Times.Once());
            repository.Verify(x => x.FindAppAsync(appId.Name), Times.Never());
        }

        [Fact]
        public async Task Should_also_retrieve_app_by_id_if_retrieved_by_name_before()
        {
            repository.Setup(x => x.FindAppAsync(appId.Name)).Returns(Task.FromResult<IAppEntity>(appV1));

            await ProvideAppByName(appV1);
            await ProvideAppById(appV1);

            repository.Verify(x => x.FindAppAsync(appId.Name), Times.Once());
            repository.Verify(x => x.FindAppAsync(appId.Id), Times.Never());
        }

        [Theory]
        public async Task Should_clear_cache_for_id_after_update_event()
        {
            var apps = ProviderResults(appV1, appV2);

            repository.Setup(x => x.FindAppAsync(appId.Id)).Returns(() => Task.FromResult<IAppEntity>(apps()));

            await ProvideAppById(appV1);

            sut.Remove(appId);

            await ProvideAppById(appV2);

            repository.Verify(x => x.FindAppAsync(appId.Id), Times.Exactly(2));
        }
        
        [Theory]
        public async Task Should_clear_cache_for_name_after_update_event()
        {
            var apps = ProviderResults(appV1, appV2);

            repository.Setup(x => x.FindAppAsync(appId.Name)).Returns(() => Task.FromResult<IAppEntity>(apps()));

            await ProvideAppByName(appV1);

            sut.Remove(appId);

            await ProvideAppByName(appV2);

            repository.Verify(x => x.FindAppAsync(appId.Name), Times.Exactly(2));
        }

        private async Task ProvideAppById(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByIdAsync(appId.Id));
        }

        private async Task ProvideAppByName(IAppEntity app)
        {
            Assert.Equal(app, await sut.FindAppByNameAsync(appId.Name));
        }

        private static Func<T> ProviderResults<T>(params T[] items)
        {
            var index = 0;

            return () => items[index++];
        }
    }
}
